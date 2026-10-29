namespace iMasjid.Application.DTOs.Requests.Roles;

public abstract class RoleBaseDto
{
    /// <example>Manager</example>
    public string Name { get; set; } = string.Empty;

    /// <example>Manages users and reports within the system.</example>
    public string? Description { get; set; }
}

public class CreateRoleRequest : RoleBaseDto
{
    /// <example>["Users.Read", "Users.Create", "Reports.View"]</example>
    public IReadOnlyList<string>? Permissions { get; set; }
}

public class UpdateRoleRequest
{
    /// <example>Manages all users and generates reports.</example>
    public string? Description { get; set; }
}

public class UpdateRolePermissionsRequest
{
    /// <example>["Users.Read", "Users.Update", "Reports.Export"]</example>
    public IReadOnlyList<string> Permissions { get; set; } = [];
}
