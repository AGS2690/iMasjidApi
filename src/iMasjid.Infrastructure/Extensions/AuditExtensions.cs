using Microsoft.Extensions.DependencyInjection;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Infrastructure.Services;

namespace iMasjid.Infrastructure.Extensions;

public static class AuditExtensions
{
    public static IServiceCollection AddAuditServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}
