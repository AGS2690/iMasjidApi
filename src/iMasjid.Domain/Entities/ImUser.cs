using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImUser : AuditableEntity<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public bool TwoFactorEnabled { get; set; } = false;
    public DateTime? LastLoginDate { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<ImUserRole> ImUserRoles { get; set; } = new List<ImUserRole>();
    public ICollection<ImRefreshToken> ImRefreshTokens { get; set; } = new List<ImRefreshToken>();
    public ICollection<ImUserSession> ImUserSessions { get; set; } = new List<ImUserSession>();
    public ICollection<ImLoginHistory> ImLoginHistories { get; set; } = new List<ImLoginHistory>();
    public ICollection<ImUserOtp> ImUserOtps { get; set; } = new List<ImUserOtp>();
}
