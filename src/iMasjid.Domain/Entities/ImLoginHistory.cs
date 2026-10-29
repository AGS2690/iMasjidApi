using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImLoginHistory : BaseEntity<Guid>
{
    public Guid? UserId { get; set; }
    public string UserNameOrEmail { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }

    public ImUser? User { get; set; }
}
