using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImPermission : AuditableEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<ImRolePermission> ImRolePermissions { get; set; } = new List<ImRolePermission>();
}
