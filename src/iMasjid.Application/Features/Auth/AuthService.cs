using iMasjid.Application.DTOs.Requests.Auth;
using iMasjid.Application.DTOs.Responses.Auth;
using iMasjid.Application.DTOs.Responses.Users;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Domain.Constants;
using iMasjid.Domain.Entities;
using iMasjid.Domain.Enums;
using iMasjid.Domain.Exceptions;

namespace iMasjid.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IGenericRepository<ImLoginHistory> _loginHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IOtpService _otpService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IBackgroundJobService _backgroundJobService;

    public AuthService(
        IUserRepository userRepository,
        IGenericRepository<ImLoginHistory> loginHistoryRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOtpService otpService,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IDateTimeProvider dateTimeProvider,
        IBackgroundJobService backgroundJobService)
    {
        _userRepository = userRepository;
        _loginHistoryRepository = loginHistoryRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _otpService = otpService;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _dateTimeProvider = dateTimeProvider;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedUserName = request.UserName.Trim().ToLowerInvariant();

        var isEmailUnique = await _userRepository.IsEmailUniqueAsync(normalizedEmail, null, cancellationToken);
        if (!isEmailUnique)
            throw new ConflictException("The email address is already in use.");

        var isUserNameUnique = await _userRepository.IsUserNameUniqueAsync(normalizedUserName, null, cancellationToken);
        if (!isUserNameUnique)
            throw new ConflictException("The username is already taken.");

        var roleRepo = _unitOfWork.Repository<ImRole>();
        var memberRole = (await roleRepo.FindAsync(r => r.NormalizedName == "MEMBER" && !r.IsDeleted, cancellationToken)).FirstOrDefault()
            ?? (await roleRepo.FindAsync(r => !r.IsDeleted, cancellationToken)).FirstOrDefault();

        var user = new ImUser
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            UserName = normalizedUserName,
            Email = normalizedEmail,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            IsActive = true,
            IsEmailVerified = false,
            TwoFactorEnabled = false,
            CreatedBy = normalizedUserName
        };

        var roleNames = new List<string>();

        if (memberRole != null)
        {
            user.ImUserRoles.Add(new ImUserRole
            {
                RoleId = memberRole.Id,
                CreatedBy = normalizedUserName
            });
            roleNames.Add(memberRole.Name);
        }

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await _userRepository.AddAsync(user, cancellationToken);
        }, cancellationToken);

        await _auditService.LogAsync(
            user.Id.ToString(),
            AuditAction.Create,
            nameof(ImUser),
            user.Id.ToString(),
            null,
            new { user.UserName, user.Email, Action = "User registered / signed up" },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LastLoginDate = user.LastLoginDate,
            CreatedOn = user.CreatedOn,
            Roles = roleNames
        };
    }

    public async Task<TokenDto> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var identifier = request.UserNameOrEmail.Trim();
        var user = identifier.Contains('@')
            ? await _userRepository.GetByEmailWithRolesAndPermissionsAsync(identifier, cancellationToken)
            : await _userRepository.GetByUserNameAsync(identifier, cancellationToken);

        if (user == null)
        {
            await RecordLoginHistoryAsync(null, identifier, ipAddress, false, "Invalid username or password.", cancellationToken);
            throw new UnauthorizedException("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            await RecordLoginHistoryAsync(user.Id, identifier, ipAddress, false, "Account is deactivated.", cancellationToken);
            throw new ForbiddenException("Account is deactivated. Please contact your system administrator.");
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            await RecordLoginHistoryAsync(user.Id, identifier, ipAddress, false, "Invalid password.", cancellationToken);
            throw new UnauthorizedException("Invalid username or password.");
        }

        // If Two-Factor Authentication is enabled, trigger OTP to email and require 2FA verification
        if (user.TwoFactorEnabled)
        {
            await _otpService.GenerateAndSendOtpAsync(user, OtpPurposes.TwoFactor, ipAddress, cancellationToken);

            return new TokenDto
            {
                RequiresTwoFactor = true,
                MaskedEmail = _otpService.MaskEmail(user.Email),
                UserName = user.UserName
            };
        }

        // Standard Password Login (when Two-Factor is disabled)
        return await CompleteLoginAsync(user, identifier, ipAddress, cancellationToken);
    }

    public async Task<OtpSentResponse> SendOtpAsync(SendOtpRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var identifier = request.UserNameOrEmail.Trim();
        var user = identifier.Contains('@')
            ? await _userRepository.GetByEmailAsync(identifier, cancellationToken)
            : await _userRepository.GetByUserNameAsync(identifier, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("No account was found matching the provided username or email address.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException("Account is deactivated. Please contact your system administrator.");
        }

        var purpose = string.IsNullOrWhiteSpace(request.Purpose) ? OtpPurposes.Login : request.Purpose.Trim();
        await _otpService.GenerateAndSendOtpAsync(user, purpose, ipAddress, cancellationToken);

        var maskedEmail = _otpService.MaskEmail(user.Email);
        return new OtpSentResponse
        {
            Success = true,
            MaskedEmail = maskedEmail,
            Message = $"A verification OTP code has been sent to {maskedEmail}."
        };
    }

    public async Task<TokenDto> VerifyOtpAsync(VerifyOtpRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var identifier = request.UserNameOrEmail.Trim();
        var user = identifier.Contains('@')
            ? await _userRepository.GetByEmailWithRolesAndPermissionsAsync(identifier, cancellationToken)
            : await _userRepository.GetByUserNameAsync(identifier, cancellationToken);

        if (user == null)
        {
            await RecordLoginHistoryAsync(null, identifier, ipAddress, false, "Invalid username or email for OTP verification.", cancellationToken);
            throw new UnauthorizedException("Invalid username or email.");
        }

        if (!user.IsActive)
        {
            await RecordLoginHistoryAsync(user.Id, identifier, ipAddress, false, "Account is deactivated.", cancellationToken);
            throw new ForbiddenException("Account is deactivated. Please contact your system administrator.");
        }

        var purpose = string.IsNullOrWhiteSpace(request.Purpose) ? OtpPurposes.Login : request.Purpose.Trim();
        var isValid = await _otpService.ValidateOtpAsync(user.Id, request.Otp, purpose, cancellationToken);

        // Fallback: Check if purpose was Login but user required TwoFactor (or vice versa)
        if (!isValid && purpose == OtpPurposes.Login && user.TwoFactorEnabled)
        {
            isValid = await _otpService.ValidateOtpAsync(user.Id, request.Otp, OtpPurposes.TwoFactor, cancellationToken);
        }

        if (!isValid)
        {
            await RecordLoginHistoryAsync(user.Id, identifier, ipAddress, false, "Invalid or expired OTP code.", cancellationToken);
            throw new UnauthorizedException("The OTP code provided is invalid or has expired.");
        }

        return await CompleteLoginAsync(user, identifier, ipAddress, cancellationToken);
    }

    public async Task<bool> ToggleTwoFactorAsync(Guid userId, bool enable, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new NotFoundException(nameof(ImUser), userId);

        user.TwoFactorEnabled = enable;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId.ToString(),
            AuditAction.Update,
            nameof(ImUser),
            userId.ToString(),
            null,
            new { Action = enable ? "Two-Factor Authentication Enabled" : "Two-Factor Authentication Disabled" },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        return true;
    }

    public async Task<TokenDto> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var generated = await _tokenService.RefreshTokenAsync(request.AccessToken, request.RefreshToken, ipAddress, cancellationToken);

        return new TokenDto
        {
            AccessToken = generated.AccessToken,
            RefreshToken = generated.RefreshToken,
            AccessTokenExpiresOn = generated.AccessTokenExpiresOn,
            RefreshTokenExpiresOn = generated.RefreshTokenExpiresOn
        };
    }

    public async Task<bool> RevokeTokenAsync(RevokeTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        return await _tokenService.RevokeTokenAsync(request.RefreshToken, ipAddress, cancellationToken);
    }

    public async Task<bool> LogoutAsync(string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId.HasValue)
        {
            await _tokenService.RevokeAllUserTokensAsync(_currentUserService.UserId.Value, ipAddress, cancellationToken);

            await _auditService.LogAsync(
                _currentUserService.UserId.Value.ToString(),
                AuditAction.Logout,
                nameof(ImUser),
                _currentUserService.UserId.Value.ToString(),
                null,
                new { Message = "User logged out" },
                ipAddress,
                _currentUserService.UserAgent,
                cancellationToken);

            return true;
        }

        return false;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new NotFoundException(nameof(ImUser), userId);

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new ValidationException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        _userRepository.Update(user);

        // Invalidate all existing tokens upon password change
        await _tokenService.RevokeAllUserTokensAsync(userId, _currentUserService.IpAddress, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            userId.ToString(),
            AuditAction.Update,
            nameof(ImUser),
            userId.ToString(),
            null,
            new { Action = "Password changed" },
            _currentUserService.IpAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        // Schedule delayed password change notification email asynchronously via Hangfire (3-second delay, non-blocking)
        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            var changedTime = _dateTimeProvider.UtcNow;
            var clientIp = _currentUserService.IpAddress;
            _backgroundJobService.Schedule<IEmailService>(
                emailService => emailService.SendPasswordChangedEmailAsync(user.Email, user.UserName, clientIp, changedTime, CancellationToken.None),
                TimeSpan.FromSeconds(3));
        }

        return true;
    }

    private async Task<TokenDto> CompleteLoginAsync(ImUser user, string identifier, string? ipAddress, CancellationToken cancellationToken)
    {
        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.Id, cancellationToken);

        var generated = await _tokenService.GenerateTokensAsync(user, roles, permissions, ipAddress, cancellationToken);

        user.LastLoginDate = _dateTimeProvider.UtcNow;
        _userRepository.Update(user);

        // Record successful login history
        await RecordLoginHistoryAsync(user.Id, identifier, ipAddress, true, null, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            user.Id.ToString(),
            AuditAction.Login,
            nameof(ImUser),
            user.Id.ToString(),
            null,
            new { Message = "User logged in successfully" },
            ipAddress,
            _currentUserService.UserAgent,
            cancellationToken);

        // Schedule delayed email notification asynchronously via Hangfire (3-second delay, non-blocking)
        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            var loginTime = user.LastLoginDate.Value;
            _backgroundJobService.Schedule<IEmailService>(
                emailService => emailService.SendLoginNotificationEmailAsync(user.Email, user.UserName, ipAddress, loginTime, CancellationToken.None),
                TimeSpan.FromSeconds(3));
        }

        return new TokenDto
        {
            AccessToken = generated.AccessToken,
            RefreshToken = generated.RefreshToken,
            AccessTokenExpiresOn = generated.AccessTokenExpiresOn,
            RefreshTokenExpiresOn = generated.RefreshTokenExpiresOn,
            TokenType = "Bearer",
            RequiresTwoFactor = false,
            UserName = user.UserName
        };
    }

    private async Task RecordLoginHistoryAsync(
        Guid? userId,
        string userNameOrEmail,
        string? ipAddress,
        bool isSuccess,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        try
        {
            var history = new ImLoginHistory
            {
                UserId = userId,
                UserNameOrEmail = userNameOrEmail,
                LoginTime = _dateTimeProvider.UtcNow,
                IpAddress = ipAddress,
                UserAgent = _currentUserService.UserAgent,
                IsSuccess = isSuccess,
                FailureReason = failureReason
            };

            await _loginHistoryRepository.AddAsync(history, cancellationToken);
            if (!isSuccess)
            {
                // Save failed attempt immediately
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        catch
        {
            // Do not fail login flow if login history recording encounters an issue
        }
    }
}
