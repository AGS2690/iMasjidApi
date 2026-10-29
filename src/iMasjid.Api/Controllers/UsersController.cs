using Microsoft.AspNetCore.Mvc;
using iMasjid.Api.Controllers.Base;
using iMasjid.Api.Filters;
using iMasjid.Application.Common.Models;
using iMasjid.Application.DTOs.Requests.Users;
using iMasjid.Application.DTOs.Responses.Users;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Domain.Constants;

namespace iMasjid.Api.Controllers;

public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
    {
        var users = await _userService.GetPagedListAsync(request, cancellationToken);
        return PagedOkResponse(users);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        return OkResponse(user);
    }

    [HttpGet("{id:guid}/details")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetailsById(Guid id, CancellationToken cancellationToken)
    {
        var userDetails = await _userService.GetDetailsByIdAsync(id, cancellationToken);
        return OkResponse(userDetails);
    }

    [HttpPost]
    [HasPermission(Permissions.Users.Create)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.CreateAsync(request, cancellationToken);
        return CreatedResponse($"/api/users/{user.Id}", user);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Users.Edit)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateAsync(id, request, cancellationToken);
        return OkResponse(user, "User updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Users.Delete)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, cancellationToken);
        return OkResponse("User deleted successfully.");
    }

    [HttpPut("{id:guid}/toggle-status")]
    [HasPermission(Permissions.Users.Edit)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleUserStatus(Guid id, CancellationToken cancellationToken)
    {
        await _userService.ToggleStatusAsync(id, cancellationToken);
        return OkResponse("User status toggled successfully.");
    }

    [HttpPut("{id:guid}/roles")]
    [HasPermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignUserRolesRequest request, CancellationToken cancellationToken)
    {
        await _userService.AssignRolesAsync(id, request, cancellationToken);
        return OkResponse("Roles assigned successfully.");
    }
}
