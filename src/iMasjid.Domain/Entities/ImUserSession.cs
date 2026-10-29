using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImUserSession : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresOn { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedOn { get; set; }

    public ImUser User { get; set; } = null!;
}
