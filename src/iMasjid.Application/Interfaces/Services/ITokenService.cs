using System.Security.Claims;
using iMasjid.Domain.Entities;

namespace iMasjid.Application.Interfaces.Services;

public record GeneratedToken(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresOn, DateTime RefreshTokenExpiresOn);

public interface ITokenService
{
    Task<GeneratedToken> GenerateTokensAsync(ImUser user, IReadOnlyList<string> roles, IReadOnlyList<string> permissions, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<GeneratedToken> RefreshTokenAsync(string accessToken, string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
