using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImTranslation : AuditableEntity<Guid>
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string CultureCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
