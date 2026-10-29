namespace iMasjid.Infrastructure.Services;

public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public bool EnableRateLimiting { get; set; } = true;
    public string RealIpHeader { get; set; } = "X-Forwarded-For";
    public RateLimitRule DefaultRule { get; set; } = new() { PeriodSeconds = 60, Limit = 60, QueueLimit = 5 };
    public List<EndpointRateLimitRule> EndpointRules { get; set; } = new();
}

public class RateLimitRule
{
    public int PeriodSeconds { get; set; } = 60;
    public int Limit { get; set; } = 60;
    public int QueueLimit { get; set; } = 5;
}

public class EndpointRateLimitRule : RateLimitRule
{
    /// <summary>
    /// Format: "METHOD:/path/pattern*" e.g. "POST:/api/auth/*", "GET:/api/users/*", or "/api/*"
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
}
