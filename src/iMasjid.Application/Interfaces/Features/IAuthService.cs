using iMasjid.Application.DTOs.Requests.Auth;
using iMasjid.Application.DTOs.Responses.Auth;
using iMasjid.Application.DTOs.Responses.Users;

namespace iMasjid.Application.Interfaces.Features;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<TokenDto> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<OtpSentResponse> SendOtpAsync(SendOtpRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<TokenDto> VerifyOtpAsync(VerifyOtpRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<bool> ToggleTwoFactorAsync(Guid userId, bool enable, CancellationToken cancellationToken = default);
    Task<TokenDto> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(RevokeTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
