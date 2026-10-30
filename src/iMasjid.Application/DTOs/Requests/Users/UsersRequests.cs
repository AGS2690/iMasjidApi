using iMasjid.Application.Common.Models;

namespace iMasjid.Application.DTOs.Requests.Users;

public abstract class UserBaseDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}

public class CreateUserRequest : UserBaseDto
{
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public IReadOnlyList<string>? Roles { get; set; }

    public IReadOnlyList<Guid>? RoleIds { get; set; }
}

public class UpdateUserRequest : UserBaseDto
{
}

public class AssignUserRolesRequest
{
    public IReadOnlyList<string>? Roles { get; set; }

    public IReadOnlyList<Guid>? RoleIds { get; set; }
}

public class GetUsersRequest : PagedRequest
{
    public bool? IsActive { get; set; }

    public string? Role { get; set; }
}
