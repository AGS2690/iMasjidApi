using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iMasjid.Api.Controllers.Base;
using iMasjid.Api.Filters;
using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Requests.Menus;
using iMasjid.Application.DTOs.Responses.Menus;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Domain.Constants;

namespace iMasjid.Api.Controllers;

[Authorize]
public class MenusController : BaseApiController
{
    private readonly IMenuService _menuService;

    public MenusController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet("nav")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserNavMenuResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserNavigationMenu(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized();

        var navMenu = await _menuService.GetUserNavigationMenuAsync(userId, cancellationToken);
        return OkResponse(navMenu, "User navigation menu retrieved successfully.");
    }

    [HttpGet("tree")]
    [HasPermission(Permissions.Menus.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MenuTreeNodeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMenuTree([FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var tree = await _menuService.GetTreeAsync(isActive, cancellationToken);
        return OkResponse(tree, "Menu tree retrieved successfully.");
    }

    [HttpGet]
    [HasPermission(Permissions.Menus.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MenuResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var menus = await _menuService.GetAllAsync(isActive, cancellationToken);
        return OkResponse(menus, "Menus retrieved successfully.");
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Menus.View)]
    [ProducesResponseType(typeof(ApiResponse<MenuResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var menu = await _menuService.GetByIdAsync(id, cancellationToken);
        return OkResponse(menu, "Menu retrieved successfully.");
    }

    [HttpPost]
    [HasPermission(Permissions.Menus.Create)]
    [ProducesResponseType(typeof(ApiResponse<MenuResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMenuRequest request, CancellationToken cancellationToken)
    {
        var result = await _menuService.CreateAsync(request, cancellationToken);
        return CreatedResponse($"/api/menus/{result.Id}", result, "Menu created successfully.");
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Menus.Edit)]
    [ProducesResponseType(typeof(ApiResponse<MenuResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMenuRequest request, CancellationToken cancellationToken)
    {
        var result = await _menuService.UpdateAsync(id, request, cancellationToken);
        return OkResponse(result, "Menu updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Menus.Delete)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _menuService.DeleteAsync(id, cancellationToken);
        return OkResponse("Menu deleted successfully.");
    }

    [HttpPost("{id:guid}/actions")]
    [HasPermission(Permissions.Menus.Assign)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignActions(Guid id, [FromBody] AssignMenuActionsRequest request, CancellationToken cancellationToken)
    {
        await _menuService.AssignMenuActionsAsync(id, request, cancellationToken);
        return OkResponse("Menu business actions updated successfully.");
    }
}
