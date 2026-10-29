using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImBusinessAction : AuditableEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ImMenuAction> ImMenuActions { get; set; } = new List<ImMenuAction>();
}
