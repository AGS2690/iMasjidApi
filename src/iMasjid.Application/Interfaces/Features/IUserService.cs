using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Requests.Users;
using iMasjid.Application.DTOs.Responses.Users;

namespace iMasjid.Application.Interfaces.Features;

public interface IUserService
{
    Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDetailsResponse> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserResponse>> GetPagedListAsync(GetUsersRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AssignRolesAsync(Guid id, AssignUserRolesRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid id, CancellationToken cancellationToken = default);
}
