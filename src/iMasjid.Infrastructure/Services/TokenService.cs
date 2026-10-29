using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Domain.Entities;
using iMasjid.Domain.Exceptions;
using iMasjid.Infrastructure.Persistence;

namespace iMasjid.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenService(
        IOptions<JwtSettings> jwtOptions,
        AppDbContext context,
        IDateTimeProvider dateTimeProvider)
    {
        _jwtSettings = jwtOptions.Value;
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<GeneratedToken> GenerateTokensAsync(
        ImUser user,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var jwtId = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        // Generate Refresh Token
        var refreshToken = GenerateSecureRefreshToken();
        var refreshTokenEntity = new ImRefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            JwtId = jwtId,
            IsUsed = false,
            IsRevoked = false,
            CreatedOn = now,
            CreatedByIp = ipAddress,
            ExpiresOn = now.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        await _context.ImRefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new GeneratedToken(
            accessToken,
            refreshToken,
            tokenDescriptor.Expires.Value,
            refreshTokenEntity.ExpiresOn);
    }

    public async Task<GeneratedToken> RefreshTokenAsync(
        string accessToken,
        string refreshToken,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
            throw new UnauthorizedException("Invalid access token.");

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Invalid user claims in token.");

        var storedToken = await _context.ImRefreshTokens
            .Include(r => r.User)
                .ThenInclude(u => u.ImUserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (storedToken == null)
            throw new UnauthorizedException("Refresh token not found.");

        if (storedToken.IsRevoked)
        {
            // Token Reuse Protection: If a revoked token is used, revoke all user tokens for security
            await RevokeAllUserTokensAsync(userId, ipAddress, cancellationToken);
            throw new SecurityTokenException("Attempted reuse of revoked refresh token. All active sessions have been invalidated.");
        }

        if (storedToken.IsUsed)
            throw new UnauthorizedException("Refresh token has already been used.");

        if (storedToken.IsExpired)
            throw new UnauthorizedException("Refresh token has expired.");

        if (storedToken.UserId != userId)
            throw new UnauthorizedException("Token does not belong to this user.");

        var now = _dateTimeProvider.UtcNow;
        var newRefreshToken = GenerateSecureRefreshToken();

        // Mark old token as used and record replacement
        storedToken.IsUsed = true;
        storedToken.ReplacedByToken = newRefreshToken;

        var roles = storedToken.User.ImUserRoles.Select(ur => ur.Role.Name).ToList();
        var allPermissions = await _context.ImRolePermissions
            .Where(rp => storedToken.User.ImUserRoles.Select(ur => ur.RoleId).Contains(rp.RoleId))
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Create new JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var jwtId = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, storedToken.User.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, storedToken.User.Email),
            new(JwtRegisteredClaimNames.Name, storedToken.User.FullName),
            new(JwtRegisteredClaimNames.UniqueName, storedToken.User.UserName),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in allPermissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var newAccessToken = tokenHandler.WriteToken(securityToken);

        var newRefreshTokenEntity = new ImRefreshToken
        {
            UserId = storedToken.UserId,
            Token = newRefreshToken,
            JwtId = jwtId,
            IsUsed = false,
            IsRevoked = false,
            CreatedOn = now,
            CreatedByIp = ipAddress,
            ExpiresOn = now.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        await _context.ImRefreshTokens.AddAsync(newRefreshTokenEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new GeneratedToken(
            newAccessToken,
            newRefreshToken,
            tokenDescriptor.Expires.Value,
            newRefreshTokenEntity.ExpiresOn);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var token = await _context.ImRefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);
        if (token == null || !token.IsActive)
            return false;

        token.IsRevoked = true;
        token.RevokedOn = _dateTimeProvider.UtcNow;
        token.RevokedByIp = ipAddress;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _context.ImRefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked && !r.IsUsed)
            .ToListAsync(cancellationToken);

        var now = _dateTimeProvider.UtcNow;
        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedOn = now;
            token.RevokedByIp = ipAddress;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateLifetime = false // Allow reading claims from expired access tokens during refresh
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static string GenerateSecureRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
