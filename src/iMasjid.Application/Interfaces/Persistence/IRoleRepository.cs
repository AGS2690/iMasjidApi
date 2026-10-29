using iMasjid.Domain.Entities;

namespace iMasjid.Application.Interfaces.Persistence;

public interface IRoleRepository : IGenericRepository<ImRole>
{
    Task<IReadOnlyList<ImRole>> GetAllWithPermissionsAsync(CancellationToken cancellationToken = default);
    Task<ImRole?> GetByIdWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
}
