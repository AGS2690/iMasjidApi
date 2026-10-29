using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using iMasjid.Application.Interfaces.Services;

namespace iMasjid.Infrastructure.Caching;

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;

    public DistributedCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var data = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(data))
            return default;

        return JsonSerializer.Deserialize<T>(data);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync<T>(key, cancellationToken);
        if (existing is not null)
            return existing;

        var value = await factory();
        await SetAsync(key, value, absoluteExpiration, slidingExpiration, cancellationToken);
        return value;
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions();

        if (absoluteExpiration.HasValue)
        {
            options.SetAbsoluteExpiration(absoluteExpiration.Value);
        }
        else
        {
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
        }

        if (slidingExpiration.HasValue)
        {
            options.SetSlidingExpiration(slidingExpiration.Value);
        }

        var json = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, json, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        // Note: For standard IDistributedCache, prefix removal requires Lua scripting or pattern scan on Redis
        // In this default distributed implementation, key removal can be handled via specific keys.
        await _distributedCache.RemoveAsync(prefixKey, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var data = await _distributedCache.GetAsync(key, cancellationToken);
        return data != null && data.Length > 0;
    }
}
