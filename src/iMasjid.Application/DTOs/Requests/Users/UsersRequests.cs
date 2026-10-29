using iMasjid.Application.Common.Models;

namespace iMasjid.Application.DTOs.Requests.Users;

public abstract class UserBaseDto
{
    /// <example>John</example>
    public string FirstName { get; set; } = string.Empty;

    /// <example>Doe</example>
    public string LastName { get; set; } = string.Empty;

    /// <example>+91-9876543210</example>
    public string? PhoneNumber { get; set; }
}

public class CreateUserRequest : UserBaseDto
{
    /// <example>john.doe</example>
    public string UserName { get; set; } = string.Empty;

    /// <example>john.doe@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <example>StrongPass@123</example>
    public string Password { get; set; } = string.Empty;

    /// <example>["Manager", "Viewer"]</example>
    public IReadOnlyList<string>? Roles { get; set; }

    /// <example>["c8f9b474-6afa-4e63-8878-d766e3bc6123"]</example>
    public IReadOnlyList<Guid>? RoleIds { get; set; }
}

public class UpdateUserRequest : UserBaseDto
{
}

public class AssignUserRolesRequest
{
    /// <example>["Admin", "Manager"]</example>
    public IReadOnlyList<string>? Roles { get; set; }

    /// <example>["c8f9b474-6afa-4e63-8878-d766e3bc6123"]</example>
    public IReadOnlyList<Guid>? RoleIds { get; set; }
}

public class GetUsersRequest : PagedRequest
{
    /// <example>true</example>
    public bool? IsActive { get; set; }

    /// <example>Manager</example>
    public string? Role { get; set; }
}
