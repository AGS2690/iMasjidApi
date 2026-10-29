using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Infrastructure.Caching;

namespace iMasjid.Infrastructure.Extensions;

public static class CachingExtensions
{
    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "iMasjid_";
            });
            services.AddSingleton<ICacheService, DistributedCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        return services;
    }
}
