using iMasjid.Application.DTOs.Requests.Roles;

namespace iMasjid.Application.DTOs.Responses.Roles;

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class RoleResponse : RoleBaseDto
{
    public Guid Id { get; set; }
    public string NormalizedName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public IReadOnlyList<string> Permissions { get; set; } = [];
}
