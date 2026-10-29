using Microsoft.EntityFrameworkCore;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Domain.Entities;
using iMasjid.Infrastructure.Persistence;

namespace iMasjid.Infrastructure.Repositories;

public class MenuRepository : GenericRepository<ImMenu>, IMenuRepository
{
    public MenuRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ImMenu>> GetAllWithActionsAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.ImMenuActions.Where(ma => ma.IsActive))
                .ThenInclude(ma => ma.Action)
            .Where(m => !m.IsDeleted && (!isActive.HasValue || m.IsActive == isActive.Value))
            .OrderBy(m => m.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<ImMenu?> GetByIdWithActionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.ImMenuActions.Where(ma => ma.IsActive))
                .ThenInclude(ma => ma.Action)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<ImRoleMenu>> GetRoleMenusByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.ImRoleMenus
            .AsNoTracking()
            .Include(rm => rm.ImRoleMenuActions)
                .ThenInclude(rma => rma.MenuAction)
                    .ThenInclude(ma => ma.Action)
            .Where(rm => rm.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ImRoleMenu>> GetRoleMenusByRoleIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var roleIdList = roleIds.ToList();
        return await _context.ImRoleMenus
            .AsNoTracking()
            .Include(rm => rm.Menu)
                .ThenInclude(m => m.ImMenuActions.Where(ma => ma.IsActive))
                    .ThenInclude(ma => ma.Action)
            .Include(rm => rm.ImRoleMenuActions)
                .ThenInclude(rma => rma.MenuAction)
                    .ThenInclude(ma => ma.Action)
            .Where(rm => roleIdList.Contains(rm.RoleId) && !rm.Menu.IsDeleted && rm.Menu.IsActive)
            .ToListAsync(cancellationToken);
    }
}
