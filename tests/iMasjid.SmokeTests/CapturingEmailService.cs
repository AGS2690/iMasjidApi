using System.Collections.Concurrent;
using iMasjid.Application.Interfaces.Services;

namespace iMasjid.SmokeTests;

public sealed class CapturingEmailService : IEmailService
{
    private readonly ConcurrentDictionary<string, string> _otpCodes;

    public CapturingEmailService(ConcurrentDictionary<string, string> otpCodes)
    {
        _otpCodes = otpCodes;
    }

    public Task SendLoginNotificationEmailAsync(string toEmail, string userName, string? ipAddress, DateTime loginTime, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task SendPasswordChangedEmailAsync(string toEmail, string userName, string? ipAddress, DateTime changedTime, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task SendOtpEmailAsync(string toEmail, string userName, string otpCode, string purpose, TimeSpan expiryDuration, CancellationToken cancellationToken = default)
    {
        _otpCodes[$"{toEmail}|{purpose}"] = otpCode;
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
