using Microsoft.AspNetCore.Authorization;

namespace iMasjid.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    public string Permission { get; }

    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
        Permission = permission;
    }
}
