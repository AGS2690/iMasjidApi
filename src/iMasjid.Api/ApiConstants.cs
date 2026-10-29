namespace iMasjid.Api;

/// <summary>Shared string constants used across API configuration to avoid magic strings.</summary>
public static class ApiConstants
{
    /// <summary>
    /// Name of the CORS policy applied to all endpoints.
    /// Actual allowed origins are configured in appsettings.json under "Cors:AllowedOrigins".
    /// </summary>
    public const string CorsPolicyName = "DefaultCorsPolicy";
}
