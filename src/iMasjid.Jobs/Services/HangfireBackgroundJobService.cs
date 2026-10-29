using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using iMasjid.Application.Interfaces.Services;

namespace iMasjid.Jobs.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HangfireBackgroundJobService> _logger;

    public HangfireBackgroundJobService(
        IServiceProvider serviceProvider,
        ILogger<HangfireBackgroundJobService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private IBackgroundJobClient? GetClient()
    {
        try
        {
            return _serviceProvider.GetService<IBackgroundJobClient>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to resolve IBackgroundJobClient.");
            return null;
        }
    }

    public string Enqueue(Expression<Action> methodCall)
    {
        try
        {
            return GetClient()?.Enqueue(methodCall) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enqueue background job.");
            return string.Empty;
        }
    }

    public string Enqueue<T>(Expression<Action<T>> methodCall)
    {
        try
        {
            return GetClient()?.Enqueue(methodCall) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enqueue background job.");
            return string.Empty;
        }
    }

    public string Enqueue(Expression<Func<Task>> methodCall)
    {
        try
        {
            return GetClient()?.Enqueue(methodCall) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enqueue background job.");
            return string.Empty;
        }
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        try
        {
            return GetClient()?.Enqueue(methodCall) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enqueue background job.");
            return string.Empty;
        }
    }

    public string Schedule(Expression<Action> methodCall, TimeSpan delay)
    {
        try
        {
            return GetClient()?.Schedule(methodCall, delay) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to schedule background job.");
            return string.Empty;
        }
    }

    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    {
        try
        {
            return GetClient()?.Schedule(methodCall, delay) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to schedule background job.");
            return string.Empty;
        }
    }

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        try
        {
            return GetClient()?.Schedule(methodCall, delay) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to schedule background job.");
            return string.Empty;
        }
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        try
        {
            return GetClient()?.Schedule(methodCall, delay) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to schedule background job.");
            return string.Empty;
        }
    }
}
