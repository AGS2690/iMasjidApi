using iMasjid.Domain.Entities;

namespace iMasjid.Application.Interfaces.Persistence;

public interface IUserRepository : IGenericRepository<ImUser>
{
    Task<ImUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ImUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<ImUser?> GetWithRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ImUser?> GetByEmailWithRolesAndPermissionsAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, IReadOnlyList<string>>> GetRolesForUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<bool> IsUserNameUniqueAsync(string userName, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
}
