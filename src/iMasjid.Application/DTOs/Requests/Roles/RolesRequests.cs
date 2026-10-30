namespace iMasjid.Application.DTOs.Requests.Roles;

public abstract class RoleBaseDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class CreateRoleRequest : RoleBaseDto
{
    public IReadOnlyList<string>? Permissions { get; set; }
}

public class UpdateRoleRequest
{
    public string? Description { get; set; }
}

public class UpdateRolePermissionsRequest
{
    public IReadOnlyList<string> Permissions { get; set; } = [];
}
