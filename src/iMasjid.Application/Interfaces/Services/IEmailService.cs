namespace iMasjid.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendLoginNotificationEmailAsync(string toEmail, string userName, string? ipAddress, DateTime loginTime, CancellationToken cancellationToken = default);
    Task SendPasswordChangedEmailAsync(string toEmail, string userName, string? ipAddress, DateTime changedTime, CancellationToken cancellationToken = default);
    Task SendOtpEmailAsync(string toEmail, string userName, string otpCode, string purpose, TimeSpan expiryDuration, CancellationToken cancellationToken = default);
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
