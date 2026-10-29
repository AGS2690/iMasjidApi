using iMasjid.Application.DTOs.Requests.Menus;
using iMasjid.Application.DTOs.Responses.Menus;

namespace iMasjid.Application.Interfaces.Features;

public interface IMenuService
{
    Task<IReadOnlyList<MenuResponse>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuTreeNodeResponse>> GetTreeAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<MenuResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MenuResponse> CreateAsync(CreateMenuRequest request, CancellationToken cancellationToken = default);
    Task<MenuResponse> UpdateAsync(Guid id, UpdateMenuRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AssignMenuActionsAsync(Guid menuId, AssignMenuActionsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserNavMenuResponse>> GetUserNavigationMenuAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleMenuDetailsResponse>> GetRoleMenusAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> AssignRoleMenusAsync(Guid roleId, AssignRoleMenusRequest request, CancellationToken cancellationToken = default);
}
