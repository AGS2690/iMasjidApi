namespace iMasjid.Application.DTOs.Requests.Auth;

public class LoginRequest
{
    /// <example>admin</example>
    public string UserNameOrEmail { get; set; } = string.Empty;

    /// <example>Voltas@123</example>
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    /// <example>John</example>
    public string FirstName { get; set; } = string.Empty;

    /// <example>Doe</example>
    public string LastName { get; set; } = string.Empty;

    /// <example>john.doe</example>
    public string UserName { get; set; } = string.Empty;

    /// <example>john.doe@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <example>+91-9876543210</example>
    public string? PhoneNumber { get; set; }

    /// <example>StrongPass@123</example>
    public string Password { get; set; } = string.Empty;

    /// <example>StrongPass@123</example>
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class SendOtpRequest
{
    /// <example>admin</example>
    public string UserNameOrEmail { get; set; } = string.Empty;

    /// <example>Login</example>
    public string Purpose { get; set; } = "Login";
}

public class VerifyOtpRequest
{
    /// <example>admin</example>
    public string UserNameOrEmail { get; set; } = string.Empty;

    /// <example>123456</example>
    public string Otp { get; set; } = string.Empty;

    /// <example>Login</example>
    public string Purpose { get; set; } = "Login";
}

public class ToggleTwoFactorRequest
{
    /// <example>true</example>
    public bool Enable { get; set; }
}

public class RevokeTokenRequest
{
    /// <example>dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4gc2FtcGxl</example>
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenRequest : RevokeTokenRequest
{
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.SflKxw</example>
    public string AccessToken { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    /// <example>Voltas@123</example>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <example>NewPass@456</example>
    public string NewPassword { get; set; } = string.Empty;

    /// <example>NewPass@456</example>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
