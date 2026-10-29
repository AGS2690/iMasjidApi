using System.Net.Http.Headers;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using iMasjid.Domain.Constants;
using iMasjid.Jobs.Configurations;

namespace iMasjid.Jobs.Security;

public class HangfireCustomBasicAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // 1. Check if user is already authenticated via JWT / Cookie with SuperAdmin role
        if (httpContext.User.Identity?.IsAuthenticated == true &&
            httpContext.User.IsInRole(Roles.SuperAdmin))
        {
            return true;
        }

        // 2. Check HTTP Basic Authentication Header
        var authHeader = httpContext.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var headerParameter = authHeader["Basic ".Length..].Trim();
                var credentialsBytes = Convert.FromBase64String(headerParameter);
                var credentials = Encoding.UTF8.GetString(credentialsBytes).Split(':', 2);

                if (credentials.Length == 2)
                {
                    var username = credentials[0];
                    var password = credentials[1];

                    var hangfireSettings = httpContext.RequestServices.GetRequiredService<IOptions<HangfireSettings>>().Value;

                    if (username == hangfireSettings.DashboardUsername &&
                        password == hangfireSettings.DashboardPassword)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Invalid credentials format
            }
        }

        // 3. Challenge browser with HTTP 401 & WWW-Authenticate header
        httpContext.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Private Dashboard\"");
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        return false;
    }
}
