namespace iMasjid.Infrastructure.Services;

public sealed class SecuritySettings
{
    public const string SectionName = "Security";

    public string OtpHashKey { get; set; } = string.Empty;
}
