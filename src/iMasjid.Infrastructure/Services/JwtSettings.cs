namespace iMasjid.Infrastructure.Services;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public List<string> Audiences { get; set; } = new();
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;

    public List<string> GetValidAudiences()
    {
        var list = new List<string>();
        if (Audiences != null && Audiences.Count > 0)
        {
            list.AddRange(Audiences.Where(a => !string.IsNullOrWhiteSpace(a)));
        }

        if (!string.IsNullOrWhiteSpace(Audience) && !list.Contains(Audience))
        {
            list.Add(Audience);
        }

        return list;
    }
}
