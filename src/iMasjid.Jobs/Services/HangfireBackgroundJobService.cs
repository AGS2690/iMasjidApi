using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using iMasjid.Application.Interfaces.Services;

namespace iMasjid.Jobs.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly IServiceProvider _serviceProvider;

    public HangfireBackgroundJobService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private IBackgroundJobClient GetClient() =>
        _serviceProvider.GetRequiredService<IBackgroundJobClient>();

    public string Enqueue(Expression<Action> methodCall)
        => GetClient().Enqueue(methodCall);

    public string Enqueue<T>(Expression<Action<T>> methodCall)
        => GetClient().Enqueue(methodCall);

    public string Enqueue(Expression<Func<Task>> methodCall)
        => GetClient().Enqueue(methodCall);

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
        => GetClient().Enqueue(methodCall);

    public string Schedule(Expression<Action> methodCall, TimeSpan delay)
        => GetClient().Schedule(methodCall, delay);

    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        => GetClient().Schedule(methodCall, delay);

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
        => GetClient().Schedule(methodCall, delay);

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
        => GetClient().Schedule(methodCall, delay);
}
