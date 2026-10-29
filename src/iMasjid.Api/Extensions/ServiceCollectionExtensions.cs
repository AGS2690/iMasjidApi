using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using iMasjid.Api.Filters;
using iMasjid.Application.Common.Models;
using iMasjid.Infrastructure.Persistence;
using iMasjid.Infrastructure.Services;

namespace iMasjid.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Controllers & Consistent API Model Validation Response
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .SelectMany(kvp => kvp.Value!.Errors.Select(err => new ErrorDetail(kvp.Key, err.ErrorMessage)))
                        .ToList();

                    var correlationId = context.HttpContext.Items.TryGetValue("CorrelationId", out var id)
                        ? id?.ToString()
                        : context.HttpContext.TraceIdentifier;

                    var response = ApiResponse.FailureResult("Validation failed.", errors, correlationId);
                    return new BadRequestObjectResult(response);
                };
            });

        // 2. Permission-based Authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // 3. CORS – strictly allow ONLY the origins listed in appsettings.json "Cors:AllowedOrigins"
        var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        if (corsOrigins == null || corsOrigins.Length == 0)
            throw new InvalidOperationException(
                "CORS misconfiguration: 'Cors:AllowedOrigins' in appsettings.json is missing or empty. " +
                "Add at least one allowed origin to prevent all cross-origin requests from being blocked.");

        services.AddCors(options =>
        {
            options.AddPolicy(ApiConstants.CorsPolicyName, policy =>
            {
                policy
                    .WithOrigins(corsOrigins)   // Only exact origins from appsettings are allowed
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();        // Required for cookies / Authorization header
            });
        });

        // 4. Native ASP.NET Core Rate Limiting configured dynamically from appsettings
        var rateLimitingSettings = new RateLimitingSettings();
        configuration.GetSection(RateLimitingSettings.SectionName).Bind(rateLimitingSettings);
        services.Configure<RateLimitingSettings>(configuration.GetSection(RateLimitingSettings.SectionName));

        if (rateLimitingSettings.EnableRateLimiting)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                    }

                    var correlationId = context.HttpContext.Items.TryGetValue("CorrelationId", out var id)
                        ? id?.ToString()
                        : context.HttpContext.TraceIdentifier;

                    var response = ApiResponse.FailureResult("Too many requests. Please slow down and try again later.", correlationId: correlationId);
                    await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: token);
                };

                // Dynamic Endpoint Rate Limiting Policy (Enforces rules configured in appsettings.json)
                options.AddPolicy("DynamicEndpointRateLimitPolicy", httpContext =>
                {
                    var clientIp = GetClientIp(httpContext, rateLimitingSettings.RealIpHeader);
                    var method = httpContext.Request.Method;
                    var path = httpContext.Request.Path.Value ?? string.Empty;

                    // Match against configured EndpointRules
                    var matchedRule = rateLimitingSettings.EndpointRules
                        .FirstOrDefault(rule => MatchesEndpointRule(rule.Endpoint, method, path));

                    var effectiveRule = matchedRule ?? rateLimitingSettings.DefaultRule;
                    var partitionKey = $"{clientIp}_{matchedRule?.Endpoint ?? "default"}";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = effectiveRule.Limit,
                            Window = TimeSpan.FromSeconds(effectiveRule.PeriodSeconds),
                            QueueLimit = effectiveRule.QueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });
                });
            });
        }

        // 5. Swagger / OpenAPI with JWT Bearer Definition
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "iMasjid API"
            });

            // Include XML doc comments so <example> tags on DTOs appear in swagger.json
            var apiXml = Path.Combine(AppContext.BaseDirectory, "iMasjid.Api.xml");
            if (File.Exists(apiXml)) options.IncludeXmlComments(apiXml, includeControllerXmlComments: true);

            var appXml = Path.Combine(AppContext.BaseDirectory, "iMasjid.Application.xml");
            if (File.Exists(appXml)) options.IncludeXmlComments(appXml);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT Bearer token only (do not prepend 'Bearer ' prefix). Example: eyJhbGciOi..."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // 6. Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("Database");

        return services;
    }

    private static string GetClientIp(HttpContext httpContext, string realIpHeader)
    {
        if (!string.IsNullOrWhiteSpace(realIpHeader) &&
            httpContext.Request.Headers.TryGetValue(realIpHeader, out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString().Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_client";
    }

    private static bool MatchesEndpointRule(string ruleEndpoint, string httpMethod, string requestPath)
    {
        if (string.IsNullOrWhiteSpace(ruleEndpoint) || ruleEndpoint == "*")
            return true;

        string targetMethod = "*";
        string targetPath = ruleEndpoint.Trim();

        if (ruleEndpoint.Contains(':'))
        {
            var parts = ruleEndpoint.Split(':', 2);
            targetMethod = parts[0].Trim();
            targetPath = parts[1].Trim();
        }

        // Method check
        if (!targetMethod.Equals("*", StringComparison.OrdinalIgnoreCase) &&
            !targetMethod.Equals(httpMethod, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Path check (support prefix wildcard /api/auth/*)
        if (targetPath.EndsWith('*'))
        {
            var prefix = targetPath.TrimEnd('*');
            return requestPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        return requestPath.Equals(targetPath, StringComparison.OrdinalIgnoreCase);
    }
}
