using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using iMasjid.Infrastructure.Extensions;

namespace iMasjid.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddSecurityServices(configuration);
        services.AddCachingServices(configuration);
        services.AddAuditServices();

        return services;
    }
}
