using iMasjid.Domain.Entities;

namespace iMasjid.Application.Interfaces.Persistence;

public interface IAppDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
