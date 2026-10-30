namespace iMasjid.Application.DTOs.Requests.Auth;

public class LoginRequest
{
    public string UserNameOrEmail { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Password { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;
}

public class SendOtpRequest
{
    public string UserNameOrEmail { get; set; } = string.Empty;

    public string Purpose { get; set; } = "Login";
}

public class VerifyOtpRequest
{
    public string UserNameOrEmail { get; set; } = string.Empty;

    public string Otp { get; set; } = string.Empty;

    public string Purpose { get; set; } = "Login";
}

public class ToggleTwoFactorRequest
{
    public bool Enable { get; set; }
}

public class RevokeTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenRequest : RevokeTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmNewPassword { get; set; } = string.Empty;
}
