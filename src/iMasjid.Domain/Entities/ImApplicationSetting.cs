using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImApplicationSetting : AuditableEntity<Guid>
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = "General";
    public bool IsEncrypted { get; set; } = false;
}
