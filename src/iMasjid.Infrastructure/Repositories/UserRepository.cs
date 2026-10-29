using Microsoft.EntityFrameworkCore;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Domain.Entities;
using iMasjid.Infrastructure.Persistence;

namespace iMasjid.Infrastructure.Repositories;

public class UserRepository : GenericRepository<ImUser>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ImUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized && !u.IsDeleted, cancellationToken);
    }

    public async Task<ImUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalized = userName.Trim().ToLowerInvariant();
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == normalized && !u.IsDeleted, cancellationToken);
    }

    public async Task<ImUser?> GetWithRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(u => u.ImUserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.ImRolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
    }

    public async Task<ImUser?> GetByEmailWithRolesAndPermissionsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _dbSet
            .AsNoTracking()
            .Include(u => u.ImUserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.ImRolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized && !u.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Role-based Permissions only
        return await _context.ImUserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId && ur.Role.IsActive && !ur.Role.IsDeleted)
            .SelectMany(ur => ur.Role.ImRolePermissions)
            .Where(rp => rp.Permission.IsActive && !rp.Permission.IsDeleted)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ImUserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId && ur.Role.IsActive && !ur.Role.IsDeleted)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, IReadOnlyList<string>>> GetRolesForUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var list = await _context.ImUserRoles
            .AsNoTracking()
            .Where(ur => userIds.Contains(ur.UserId) && ur.Role.IsActive && !ur.Role.IsDeleted)
            .Select(ur => new { ur.UserId, RoleName = ur.Role.Name })
            .ToListAsync(cancellationToken);

        return list
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g.Select(x => x.RoleName).Distinct().ToList());
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return !await _dbSet
            .AsNoTracking()
            .AnyAsync(u => u.Email.ToLower() == normalized && (!excludeUserId.HasValue || u.Id != excludeUserId.Value) && !u.IsDeleted, cancellationToken);
    }

    public async Task<bool> IsUserNameUniqueAsync(string userName, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var normalized = userName.Trim().ToLowerInvariant();
        return !await _dbSet
            .AsNoTracking()
            .AnyAsync(u => u.UserName.ToLower() == normalized && (!excludeUserId.HasValue || u.Id != excludeUserId.Value) && !u.IsDeleted, cancellationToken);
    }
}
