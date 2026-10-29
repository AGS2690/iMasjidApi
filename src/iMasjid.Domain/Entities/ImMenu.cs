using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImMenu : AuditableEntity<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public Guid? ParentId { get; set; }
    public int OrderIndex { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ImMenu? ParentMenu { get; set; }
    public ICollection<ImMenu> SubMenus { get; set; } = new List<ImMenu>();
    public ICollection<ImMenuAction> ImMenuActions { get; set; } = new List<ImMenuAction>();
    public ICollection<ImRoleMenu> ImRoleMenus { get; set; } = new List<ImRoleMenu>();
}
