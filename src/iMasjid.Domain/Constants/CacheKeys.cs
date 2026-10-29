namespace iMasjid.Domain.Constants;

public static class CacheKeys
{
    public const string UserPermissionsPrefix = "user-perms:";
    public const string RolePermissionsPrefix = "role-perms:";
    public const string AppSettings = "app-settings";

    public static string UserPermissions(Guid userId) => $"{UserPermissionsPrefix}{userId}";
    public static string RolePermissions(Guid roleId) => $"{RolePermissionsPrefix}{roleId}";
    public static string Setting(string key) => $"setting:{key.ToLowerInvariant()}";
}
