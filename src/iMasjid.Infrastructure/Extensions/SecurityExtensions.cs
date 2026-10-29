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

        services.Configure<JwtSettings>(options =>
        {
            options.SecretKey = jwtSettings.SecretKey;
            options.Issuer = jwtSettings.Issuer;
            options.Audience = jwtSettings.Audience;
            options.Audiences = jwtSettings.Audiences;
            options.AccessTokenExpirationMinutes = jwtSettings.AccessTokenExpirationMinutes;
            options.RefreshTokenExpirationDays = jwtSettings.RefreshTokenExpirationDays;
        });

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        var key = Encoding.UTF8.GetBytes(string.IsNullOrEmpty(jwtSettings.SecretKey) ? "A_Strong_Default_Secret_Key_For_Testing_Only_32_Bytes_Long" : jwtSettings.SecretKey);
        var validAudiences = jwtSettings.GetValidAudiences();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
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
