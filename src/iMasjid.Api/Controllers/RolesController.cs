using Microsoft.AspNetCore.Mvc;
using iMasjid.Api.Controllers.Base;
using iMasjid.Api.Filters;
using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Requests.Menus;
using iMasjid.Application.DTOs.Requests.Roles;
using iMasjid.Application.DTOs.Responses.Menus;
using iMasjid.Application.DTOs.Responses.Roles;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Domain.Constants;

namespace iMasjid.Api.Controllers;

public class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;
    private readonly IMenuService _menuService;

    public RolesController(IRoleService roleService, IMenuService menuService)
    {
        _roleService = roleService;
        _menuService = menuService;
    }

    [HttpGet]
    [HasPermission(Permissions.Roles.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RoleResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetAllAsync(cancellationToken);
        return OkResponse(roles);
    }

    [HttpGet("permissions")]
    [HasPermission(Permissions.Roles.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PermissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissions(CancellationToken cancellationToken)
    {
        var permissions = await _roleService.GetAllPermissionsAsync(cancellationToken);
        return OkResponse(permissions);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Roles.View)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(Guid id, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetByIdAsync(id, cancellationToken);
        return OkResponse(role);
    }

    [HttpPost]
    [HasPermission(Permissions.Roles.Create)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await _roleService.CreateAsync(request, cancellationToken);
        return CreatedResponse($"/api/roles/{role.Id}", role);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Roles.Edit)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await _roleService.UpdateAsync(id, request, cancellationToken);
        return OkResponse(role, "Role updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Roles.Delete)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        await _roleService.DeleteAsync(id, cancellationToken);
        return OkResponse("Role deleted successfully.");
    }

    [HttpPut("{id:guid}/permissions")]
    [HasPermission(Permissions.Roles.ManagePermissions)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRolePermissions(Guid id, [FromBody] UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        await _roleService.UpdatePermissionsAsync(id, request, cancellationToken);
        return OkResponse("Role permissions updated successfully.");
    }

    [HttpGet("{id:guid}/menus")]
    [HasPermission(Permissions.Roles.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RoleMenuDetailsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleMenus(Guid id, CancellationToken cancellationToken)
    {
        var roleMenus = await _menuService.GetRoleMenusAsync(id, cancellationToken);
        return OkResponse(roleMenus, "Role menus retrieved successfully.");
    }

    [HttpPut("{id:guid}/menus")]
    [HasPermission(Permissions.Menus.Assign)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRoleMenus(Guid id, [FromBody] AssignRoleMenusRequest request, CancellationToken cancellationToken)
    {
        await _menuService.AssignRoleMenusAsync(id, request, cancellationToken);
        return OkResponse("Role menus and action permissions updated successfully.");
    }
}
