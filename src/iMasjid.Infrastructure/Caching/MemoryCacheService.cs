using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using iMasjid.Application.Interfaces.Services;

namespace iMasjid.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private static readonly ConcurrentDictionary<string, byte> Keys = new();

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out T? existing) && existing is not null)
        {
            return existing;
        }

        var value = await factory();
        await SetAsync(key, value, absoluteExpiration, slidingExpiration, cancellationToken);
        return value;
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions();

        if (absoluteExpiration.HasValue)
        {
            options.SetAbsoluteExpiration(absoluteExpiration.Value);
        }
        else
        {
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // Default 30 min absolute
        }

        if (slidingExpiration.HasValue)
        {
            options.SetSlidingExpiration(slidingExpiration.Value);
        }

        options.RegisterPostEvictionCallback((k, _, _, _) =>
        {
            Keys.TryRemove(k.ToString()!, out _);
        });

        _memoryCache.Set(key, value, options);
        Keys.TryAdd(key, 0);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        Keys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        var matchingKeys = Keys.Keys.Where(k => k.StartsWith(prefixKey, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var key in matchingKeys)
        {
            _memoryCache.Remove(key);
            Keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var exists = _memoryCache.TryGetValue(key, out _);
        return Task.FromResult(exists);
    }
}
