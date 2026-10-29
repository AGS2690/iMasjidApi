using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImRefreshToken : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }
    public DateTime ExpiresOn { get; set; }
    public DateTime? RevokedOn { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public bool IsActive => !IsRevoked && !IsUsed && !IsExpired;

    public ImUser User { get; set; } = null!;
}
