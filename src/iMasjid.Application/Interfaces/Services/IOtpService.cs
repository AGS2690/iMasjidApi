using iMasjid.Domain.Entities;

namespace iMasjid.Application.Interfaces.Services;

public interface IOtpService
{
    Task<string> GenerateAndSendOtpAsync(ImUser user, string purpose, string? ipAddress, CancellationToken cancellationToken = default);
    Task<bool> ValidateOtpAsync(Guid userId, string otpCode, string purpose, CancellationToken cancellationToken = default);
    string MaskEmail(string email);
}
