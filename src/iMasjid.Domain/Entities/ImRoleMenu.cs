using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImRoleMenu : BaseEntity<Guid>
{
    public Guid RoleId { get; set; }
    public ImRole Role { get; set; } = null!;

    public Guid MenuId { get; set; }
    public ImMenu Menu { get; set; } = null!;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    public ICollection<ImRoleMenuAction> ImRoleMenuActions { get; set; } = new List<ImRoleMenuAction>();
}
