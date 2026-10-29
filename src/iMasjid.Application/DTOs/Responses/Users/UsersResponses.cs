using iMasjid.Application.DTOs.Requests.Users;

namespace iMasjid.Application.DTOs.Responses.Users;

public class UserResponse : UserBaseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
}

public class UserDetailsResponse
{
    public UserResponse User { get; set; } = default!;
    public IReadOnlyList<string> Roles { get; set; } = [];
    public IReadOnlyList<string> Permissions { get; set; } = [];
}
