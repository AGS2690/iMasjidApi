namespace iMasjid.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
    bool IsAuthenticated { get; }
    string CultureCode { get; }
}
