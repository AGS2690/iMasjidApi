using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using iMasjid.Application.Interfaces.Features;
using iMasjid.Domain.Constants;

namespace iMasjid.Api.Filters;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            return;
        }

        if (context.User.IsInRole(Roles.SuperAdmin))
        {
            context.Succeed(requirement);
            return;
        }

        var permissionsFromClaims = context.User.FindAll("permission").Select(c => c.Value);
        if (permissionsFromClaims.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? context.User.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var effectivePermissions = await userService.GetEffectivePermissionsAsync(userId);

            if (effectivePermissions.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
        }
    }
}
