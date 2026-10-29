using iMasjid.Domain.Common;
using iMasjid.Domain.Enums;

namespace iMasjid.Domain.Entities;

public class ImAuditLog : BaseEntity<Guid>
{
    public string? UserId { get; set; }
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
