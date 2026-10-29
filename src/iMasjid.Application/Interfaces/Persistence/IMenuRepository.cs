using iMasjid.Domain.Entities;

namespace iMasjid.Application.Interfaces.Persistence;

public interface IMenuRepository : IGenericRepository<ImMenu>
{
    Task<IReadOnlyList<ImMenu>> GetAllWithActionsAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<ImMenu?> GetByIdWithActionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImRoleMenu>> GetRoleMenusByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImRoleMenu>> GetRoleMenusByRoleIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);
}
