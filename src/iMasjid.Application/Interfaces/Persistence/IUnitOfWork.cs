namespace iMasjid.Application.Interfaces.Persistence;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : class;
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task ExecuteTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
    Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default);
}
