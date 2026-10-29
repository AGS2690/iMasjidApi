namespace iMasjid.Application.DTOs.Responses.Auth;

public class TokenDto
{
    public bool RequiresTwoFactor { get; set; } = false;
    public string? MaskedEmail { get; set; }
    public string? UserName { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresOn { get; set; }
    public DateTime RefreshTokenExpiresOn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

public class OtpSentResponse
{
    public bool Success { get; set; } = true;
    public string MaskedEmail { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
