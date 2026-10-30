using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Infrastructure.Services;

namespace iMasjid.Infrastructure.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        var jwtSettings = new JwtSettings();

        jwtSettings.SecretKey = jwtSection["SecretKey"] ?? string.Empty;
        jwtSettings.Issuer = jwtSection["Issuer"] ?? string.Empty;
        jwtSettings.AccessTokenExpirationMinutes = int.TryParse(jwtSection["AccessTokenExpirationMinutes"], out var accExp) ? accExp : 15;
        jwtSettings.RefreshTokenExpirationDays = int.TryParse(jwtSection["RefreshTokenExpirationDays"], out var refExp) ? refExp : 7;

        var audienceChildren = jwtSection.GetSection("Audience").GetChildren().Select(c => c.Value).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
        if (audienceChildren.Count > 0)
        {
            jwtSettings.Audiences = audienceChildren!;
            jwtSettings.Audience = audienceChildren.First()!;
        }
        else
        {
            jwtSettings.Audience = jwtSection["Audience"] ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(jwtSettings.Audience))
                jwtSettings.Audiences = new List<string> { jwtSettings.Audience };
        }

        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
            throw new InvalidOperationException("Jwt:SecretKey must be configured with at least 32 UTF-8 bytes.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            throw new InvalidOperationException("Jwt:Issuer must be configured.");

        if (jwtSettings.GetValidAudiences().Count == 0)
            throw new InvalidOperationException("Jwt:Audience must contain at least one configured audience.");

        if (jwtSettings.AccessTokenExpirationMinutes <= 0 || jwtSettings.RefreshTokenExpirationDays <= 0)
            throw new InvalidOperationException("JWT expiration settings must be greater than zero.");

        var otpHashKey = configuration[$"{SecuritySettings.SectionName}:OtpHashKey"];
        if (string.IsNullOrWhiteSpace(otpHashKey) || Encoding.UTF8.GetByteCount(otpHashKey) < 32)
            throw new InvalidOperationException("Security:OtpHashKey must be configured with at least 32 UTF-8 bytes.");

        services.Configure<JwtSettings>(options =>
        {
            options.SecretKey = jwtSettings.SecretKey;
            options.Issuer = jwtSettings.Issuer;
            options.Audience = jwtSettings.Audience;
            options.Audiences = jwtSettings.Audiences;
            options.AccessTokenExpirationMinutes = jwtSettings.AccessTokenExpirationMinutes;
            options.RefreshTokenExpirationDays = jwtSettings.RefreshTokenExpirationDays;
        });
        services.Configure<SecuritySettings>(options => options.OtpHashKey = otpHashKey);

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
        var validAudiences = jwtSettings.GetValidAudiences();
        var environment = configuration["ASPNETCORE_ENVIRONMENT"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = !string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudiences = validAudiences.Count > 0 ? validAudiences : null,
                ValidAudience = validAudiences.Count == 1 ? validAudiences[0] : null,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
