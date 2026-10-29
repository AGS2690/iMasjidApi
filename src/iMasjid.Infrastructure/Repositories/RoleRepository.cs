using Microsoft.EntityFrameworkCore;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Domain.Entities;
using iMasjid.Infrastructure.Persistence;

namespace iMasjid.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<ImRole>, IRoleRepository
{
    public RoleRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ImRole>> GetAllWithPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .Include(r => r.ImRolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ImRole?> GetByIdWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.ImRolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.ImRolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId && rp.Permission.IsActive && !rp.Permission.IsDeleted)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
