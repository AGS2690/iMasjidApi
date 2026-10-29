using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImRoleMenuAction : BaseEntity<Guid>
{
    public Guid RoleMenuId { get; set; }
    public ImRoleMenu RoleMenu { get; set; } = null!;

    public Guid MenuActionId { get; set; }
    public ImMenuAction MenuAction { get; set; } = null!;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}
