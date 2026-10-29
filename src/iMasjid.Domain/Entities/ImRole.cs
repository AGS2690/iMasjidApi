using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImRole : AuditableEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ImUserRole> ImUserRoles { get; set; } = new List<ImUserRole>();
    public ICollection<ImRolePermission> ImRolePermissions { get; set; } = new List<ImRolePermission>();
    public ICollection<ImRoleMenu> ImRoleMenus { get; set; } = new List<ImRoleMenu>();
}
