using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using iMasjid.Application.Interfaces.Services;

namespace iMasjid.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User?.FindFirst("sub")?.Value;

            return Guid.TryParse(userId, out var guid) ? guid : null;
        }
    }

    public string? UserName => User?.FindFirst(ClaimTypes.Name)?.Value
                               ?? User?.FindFirst("unique_name")?.Value;

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value
                            ?? User?.FindFirst("email")?.Value;

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

    public IReadOnlyList<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

    public IReadOnlyList<string> Permissions => User?.FindAll("permission").Select(c => c.Value).ToList() ?? new List<string>();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string CultureCode
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "id-ID";

            // 1. Query parameter (e.g. ?culture=id-ID or ?lang=id)
            var raw = context.Request.Query["culture"].ToString();
            if (string.IsNullOrWhiteSpace(raw))
                raw = context.Request.Query["lang"].ToString();

            // 2. Custom header X-Culture / X-Language
            if (string.IsNullOrWhiteSpace(raw))
                raw = context.Request.Headers["X-Culture"].ToString();
            if (string.IsNullOrWhiteSpace(raw))
                raw = context.Request.Headers["X-Language"].ToString();

            // 3. Standard Accept-Language header
            if (string.IsNullOrWhiteSpace(raw))
                raw = context.Request.Headers["Accept-Language"].ToString();

            if (string.IsNullOrWhiteSpace(raw))
                return "id-ID";

            var first = raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            if (first.StartsWith("id", StringComparison.OrdinalIgnoreCase)) return "id-ID";
            if (first.StartsWith("ar", StringComparison.OrdinalIgnoreCase)) return "ar-SA";
            if (first.StartsWith("en", StringComparison.OrdinalIgnoreCase)) return "en-US";

            return first;
        }
    }
}
