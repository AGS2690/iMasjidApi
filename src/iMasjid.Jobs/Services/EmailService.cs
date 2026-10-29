using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Jobs.Configurations;

namespace iMasjid.Jobs.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailOptions, ILogger<EmailService> logger)
    {
        _emailSettings = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendLoginNotificationEmailAsync(
        string toEmail,
        string userName,
        string? ipAddress,
        DateTime loginTime,
        CancellationToken cancellationToken = default)
    {
        var subject = "Security Alert: New Login to Your Account";
        var body = $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;"">
            <h2 style=""color: #2c3e50;"">Security Alert: Successful Login</h2>
            <p>Hello <strong>{WebUtility.HtmlEncode(userName)}</strong>,</p>
            <p>Your account was just logged into on <strong>{loginTime:yyyy-MM-dd HH:mm:ss UTC}</strong>.</p>
            <table style=""width: 100%; border-collapse: collapse; margin: 20px 0;"">
                <tr>
                    <td style=""padding: 8px; border-bottom: 1px solid #ddd; color: #7f8c8d;"">IP Address:</td>
                    <td style=""padding: 8px; border-bottom: 1px solid #ddd; font-weight: bold;"">{WebUtility.HtmlEncode(ipAddress ?? "Unknown")}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px; border-bottom: 1px solid #ddd; color: #7f8c8d;"">Timestamp:</td>
                    <td style=""padding: 8px; border-bottom: 1px solid #ddd; font-weight: bold;"">{loginTime:yyyy-MM-dd HH:mm:ss UTC}</td>
                </tr>
            </table>
            <p style=""color: #e74c3c; font-size: 13px;"">If this wasn't you, please change your password immediately and contact your system administrator.</p>
            <hr style=""border: none; border-top: 1px solid #eee; margin: 20px 0;"" />
            <p style=""color: #95a5a6; font-size: 12px;"">This is an automated security notification from {_emailSettings.SenderName}.</p>
        </div>";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendPasswordChangedEmailAsync(
        string toEmail,
        string userName,
        string? ipAddress,
        DateTime changedTime,
        CancellationToken cancellationToken = default)
    {
        var subject = "Security Alert: Your Password Was Changed";
        var body = $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;"">
            <h2 style=""color: #c0392b;"">Security Alert: Password Changed</h2>
            <p>Hello <strong>{WebUtility.HtmlEncode(userName)}</strong>,</p>
            <p>Your account password was successfully updated on <strong>{changedTime:yyyy-MM-dd HH:mm:ss UTC}</strong>.</p>
            <table style=""width: 100%; border-collapse: collapse; margin: 20px 0;"">
                <tr>
                    <td style=""padding: 8px; border-bottom: 1px solid #ddd; color: #7f8c8d;"">Requested From IP:</td>
                    <td style=""padding: 8px; border-bottom: 1px solid #ddd; font-weight: bold;"">{WebUtility.HtmlEncode(ipAddress ?? "Unknown")}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px; border-bottom: 1px solid #ddd; color: #7f8c8d;"">Timestamp:</td>
                    <td style=""padding: 8px; border-bottom: 1px solid #ddd; font-weight: bold;"">{changedTime:yyyy-MM-dd HH:mm:ss UTC}</td>
                </tr>
            </table>
            <p style=""color: #e74c3c; font-weight: bold;"">If you did NOT perform this action, please contact your administrator immediately.</p>
            <hr style=""border: none; border-top: 1px solid #eee; margin: 20px 0;"" />
            <p style=""color: #95a5a6; font-size: 12px;"">This is an automated security notification from {_emailSettings.SenderName}.</p>
        </div>";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendOtpEmailAsync(
        string toEmail,
        string userName,
        string otpCode,
        string purpose,
        TimeSpan expiryDuration,
        CancellationToken cancellationToken = default)
    {
        var purposeTitle = purpose switch
        {
            "TwoFactor" => "Two-Factor Authentication Code",
            "PasswordReset" => "Password Reset Verification Code",
            _ => "Login Verification Code"
        };

        var subject = $"iMasjid Security: Your {purposeTitle}";
        var minutes = (int)Math.Ceiling(expiryDuration.TotalMinutes);

        var body = $@"
        <div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 550px; margin: auto; padding: 30px; border: 1px solid #e2e8f0; border-radius: 12px; background-color: #ffffff;"">
            <div style=""text-align: center; margin-bottom: 24px;"">
                <h1 style=""color: #0f766e; margin: 0; font-size: 24px;"">iMasjid Security</h1>
                <p style=""color: #64748b; font-size: 14px; margin-top: 4px;"">{purposeTitle}</p>
            </div>
            <p style=""color: #334155; font-size: 15px;"">Assalamu Alaikum / Hello <strong>{WebUtility.HtmlEncode(userName)}</strong>,</p>
            <p style=""color: #475569; font-size: 14px; line-height: 1.5;"">Use the following One-Time Password (OTP) to complete your verification for iMasjid:</p>
            
            <div style=""margin: 28px 0; text-align: center;"">
                <div style=""display: inline-block; padding: 14px 32px; background: #f0fdf4; border: 2px dashed #16a34a; border-radius: 8px; font-size: 32px; font-weight: 800; letter-spacing: 6px; color: #15803d; font-family: Consolas, monospace;"">
                    {otpCode}
                </div>
            </div>

            <p style=""color: #64748b; font-size: 13px; text-align: center;"">
                This OTP is strictly confidential and will expire in <strong>{minutes} minutes</strong>.
            </p>
            <hr style=""border: none; border-top: 1px solid #e2e8f0; margin: 24px 0;"" />
            <p style=""color: #dc2626; font-size: 12px; line-height: 1.4;"">
                <strong>Security Alert:</strong> If you did not request this OTP or someone asks you for it, do not share it. Please contact your mosque administrator immediately.
            </p>
            <p style=""color: #94a3b8; font-size: 11px; text-align: center; margin-top: 16px;"">
                This is an automated security message from {_emailSettings.SenderName}. Please do not reply directly to this email.
            </p>
        </div>";

        _logger.LogInformation(
            "[OTP GENERATED] User: '{UserName}', Email: '{ToEmail}', Purpose: '{Purpose}', OTP: '{OtpCode}', Expires in: {Minutes} min",
            userName, toEmail, purpose, otpCode, minutes);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (!_emailSettings.Enabled || string.IsNullOrWhiteSpace(_emailSettings.SmtpServer) || _emailSettings.SmtpServer == "smtp.example.com")
        {
            _logger.LogInformation(
                "[MOCK EMAIL - SMTPServer disabled or placeholder]\nTo: {ToEmail}\nSubject: {Subject}\nTimestamp: {Time}",
                toEmail, subject, DateTime.UtcNow);
            return;
        }

        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage, cancellationToken);
            _logger.LogInformation("Email sent successfully to {ToEmail} with Subject: '{Subject}'", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail} with Subject: '{Subject}'", toEmail, subject);
            throw;
        }
    }
}
