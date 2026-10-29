using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImRolePermission : BaseEntity<Guid>
{
    public Guid RoleId { get; set; }
    public ImRole Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public ImPermission Permission { get; set; } = null!;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}
