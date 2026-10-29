using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImMenuAction : BaseEntity<Guid>
{
    public Guid MenuId { get; set; }
    public ImMenu Menu { get; set; } = null!;

    public Guid ActionId { get; set; }
    public ImBusinessAction Action { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    public ICollection<ImRoleMenuAction> ImRoleMenuActions { get; set; } = new List<ImRoleMenuAction>();
}
