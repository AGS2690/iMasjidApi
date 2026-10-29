using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using iMasjid.Api.Middleware;
using iMasjid.Jobs;

using Swashbuckle.AspNetCore.SwaggerUI;

namespace iMasjid.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApiMiddlewarePipeline(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 1. Global Exception Handling (Outer-most to catch all exceptions)
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // 2. Correlation ID (Attaches correlation ID to trace log context)
        app.UseMiddleware<CorrelationIdMiddleware>();

        // 3. Request Logging (Logs method, path, duration, status)
        app.UseMiddleware<RequestLoggingMiddleware>();

        // 4. Swagger UI (available in development and staging)
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "iMasjid API");
                c.RoutePrefix = string.Empty; // Swagger UI at root URL
                c.DocExpansion(DocExpansion.None); // Close/collapse all controller sections by default
                c.DefaultModelsExpandDepth(-1); // Close/hide Schemas section by default
                c.DisplayRequestDuration(); // Display execution duration for "Try it out" requests
            });
        }

        // 4. CORS (Must be before UseHttpsRedirection to prevent redirect CORS failures)
        app.UseCors(ApiConstants.CorsPolicyName);

        // 5. HTTPS Redirection
        app.UseHttpsRedirection();

        // 6. Routing
        app.UseRouting();

        // 7. Rate Limiter (enforces throttling configured in appsettings)
        app.UseRateLimiter();

        // 9. Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // 10. Background Jobs Dashboard (Private with Basic Auth credentials)
        app.UseBackgroundJobsDashboard();

        // 11. Endpoints & Health Checks
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = report.Status.ToString(),
                        totalDuration = report.TotalDuration.TotalMilliseconds,
                        entries = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            duration = e.Value.Duration.TotalMilliseconds,
                            description = e.Value.Description,
                            error = e.Value.Exception?.Message
                        })
                    };
                    await context.Response.WriteAsJsonAsync(response);
                }
            });
        });

        return app;
    }
}
