using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImUserOtp : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string OtpCode { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }
    public DateTime? UsedOn { get; set; }
    public int AttemptsCount { get; set; } = 0;
    public int MaxAttempts { get; set; } = 5;

    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public bool IsValid => !IsUsed && !IsExpired && AttemptsCount < MaxAttempts;

    public ImUser User { get; set; } = null!;
}
