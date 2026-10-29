using iMasjid.Application.DTOs.Requests.Roles;
using iMasjid.Application.DTOs.Responses.Roles;

namespace iMasjid.Application.Interfaces.Features;

public interface IRoleService
{
    Task<IReadOnlyList<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleResponse> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdatePermissionsAsync(Guid id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
}
