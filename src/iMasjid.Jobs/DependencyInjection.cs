using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Jobs.Configurations;
using iMasjid.Jobs.Security;
using iMasjid.Jobs.Services;

namespace iMasjid.Jobs;

public static class DependencyInjection
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailService, EmailService>();

        var hangfireSettings = new HangfireSettings();
        configuration.GetSection(HangfireSettings.SectionName).Bind(hangfireSettings);

        if (!string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Testing", StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrWhiteSpace(hangfireSettings.DashboardUsername) ||
             string.IsNullOrWhiteSpace(hangfireSettings.DashboardPassword)))
        {
            throw new InvalidOperationException("Hangfire dashboard credentials must be configured through a secret store.");
        }

        services.Configure<HangfireSettings>(configuration.GetSection(HangfireSettings.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (!string.Equals(connectionString, "InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseFilter(new AutomaticRetryAttribute { Attempts = 5 })
                .UsePostgreSqlStorage(
                    c => c.UseNpgsqlConnection(connectionString),
                    new PostgreSqlStorageOptions
                    {
                        SchemaName = "hangfire",
                        PrepareSchemaIfNecessary = true,
                        QueuePollInterval = TimeSpan.FromSeconds(5)
                    }));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = hangfireSettings.WorkerCount > 0 ? hangfireSettings.WorkerCount : Environment.ProcessorCount * 2;
            });
        }

        services.AddSingleton<IBackgroundJobService, HangfireBackgroundJobService>();

        return services;
    }

    public static IApplicationBuilder UseBackgroundJobsDashboard(this IApplicationBuilder app)
    {
        var hangfireSettings = app.ApplicationServices.GetService<IOptions<HangfireSettings>>()?.Value;
        var dashboardPath = hangfireSettings?.DashboardPath ?? "/hangfire";

        app.UseHangfireDashboard(dashboardPath, new DashboardOptions
        {
            Authorization = new[] { new HangfireCustomBasicAuthFilter() },
            DashboardTitle = "iMasjid Background Jobs Dashboard"
        });

        return app;
    }
}
