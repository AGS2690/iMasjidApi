using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Domain.Entities;
using iMasjid.Infrastructure.Persistence;

namespace iMasjid.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<OtpService> _logger;
    private readonly byte[] _otpHashKey;

    private static readonly TimeSpan DefaultOtpExpiry = TimeSpan.FromMinutes(10);

    public OtpService(
        AppDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IDateTimeProvider dateTimeProvider,
        ILogger<OtpService> logger,
        IOptions<SecuritySettings> securityOptions)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _otpHashKey = Encoding.UTF8.GetBytes(securityOptions.Value.OtpHashKey);
    }

    public async Task<string> GenerateAndSendOtpAsync(
        ImUser user,
        string purpose,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;

        // Invalidate previous unused OTPs for the same user and purpose
        var previousOtps = await _context.ImUserOtps
            .Where(o => o.UserId == user.Id && o.Purpose == purpose && !o.IsUsed && o.ExpiresOn > now)
            .ToListAsync(cancellationToken);

        foreach (var oldOtp in previousOtps)
        {
            oldOtp.IsUsed = true;
            oldOtp.UsedOn = now;
        }

        // Generate cryptographically secure 6-digit numeric code
        var otpNumber = RandomNumberGenerator.GetInt32(100000, 1000000);
        var otpCode = otpNumber.ToString();

        var otpEntity = new ImUserOtp
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            OtpCode = HashOtp(user.Id, purpose, otpCode),
            Purpose = purpose,
            ExpiresOn = now.Add(DefaultOtpExpiry),
            IsUsed = false,
            CreatedOn = now,
            CreatedByIp = ipAddress,
            AttemptsCount = 0,
            MaxAttempts = 5
        };

        await _context.ImUserOtps.AddAsync(otpEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send OTP via email
        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            await _emailService.SendOtpEmailAsync(
                user.Email,
                user.UserName,
                otpCode,
                purpose,
                DefaultOtpExpiry,
                cancellationToken);
        }
        else
        {
            _logger.LogWarning("Cannot send OTP to user '{UserName}' because email address is missing.", user.UserName);
        }

        return otpCode;
    }

    public async Task<bool> ValidateOtpAsync(
        Guid userId,
        string otpCode,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;

        var activeOtp = await _context.ImUserOtps
            .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed)
            .OrderByDescending(o => o.CreatedOn)
            .FirstOrDefaultAsync(o => o.ExpiresOn > now, cancellationToken);

        if (activeOtp == null)
        {
            _logger.LogWarning("OTP validation failed for user {UserId}: No active, unexpired OTP found for purpose '{Purpose}'.", userId, purpose);
            return false;
        }

        if (activeOtp.AttemptsCount >= activeOtp.MaxAttempts)
        {
            _logger.LogWarning("OTP validation failed for user {UserId}: Maximum attempt limit reached ({Attempts}).", userId, activeOtp.AttemptsCount);
            return false;
        }

        var suppliedHash = HashOtp(userId, purpose, otpCode);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(activeOtp.OtpCode),
                Encoding.UTF8.GetBytes(suppliedHash)))
        {
            var updated = await _context.ImUserOtps
                .Where(o => o.Id == activeOtp.Id && !o.IsUsed && o.ExpiresOn > now && o.AttemptsCount < o.MaxAttempts)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.AttemptsCount, o => o.AttemptsCount + 1), cancellationToken);

            _logger.LogWarning("Invalid OTP entered for user {UserId}; attempt update applied: {Updated}.", userId, updated == 1);
            return false;
        }

        var consumed = await _context.ImUserOtps
            .Where(o => o.Id == activeOtp.Id && !o.IsUsed && o.ExpiresOn > now && o.AttemptsCount < o.MaxAttempts)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.IsUsed, true)
                .SetProperty(o => o.UsedOn, now), cancellationToken);

        if (consumed != 1)
            return false;

        _logger.LogInformation("OTP successfully verified for user {UserId} (Purpose: '{Purpose}').", userId, purpose);
        return true;
    }

    private string HashOtp(Guid userId, string purpose, string otpCode)
    {
        var payload = $"{userId:N}|{purpose.Trim()}|{otpCode.Trim()}";
        using var hmac = new HMACSHA256(_otpHashKey);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    }

    public string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return "******";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"{localPart[0]}*@{domain}";

        var first = localPart[0];
        var last = localPart[^1];
        var masked = new string('*', Math.Min(localPart.Length - 2, 4));

        return $"{first}{masked}{last}@{domain}";
    }
}
