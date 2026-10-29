namespace iMasjid.Domain.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
        public const string ManageRoles = "Permissions.Users.ManageRoles";
    }

    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit = "Permissions.Roles.Edit";
        public const string Delete = "Permissions.Roles.Delete";
        public const string ManagePermissions = "Permissions.Roles.ManagePermissions";
    }

    public static class Menus
    {
        public const string View = "Permissions.Menus.View";
        public const string Create = "Permissions.Menus.Create";
        public const string Edit = "Permissions.Menus.Edit";
        public const string Delete = "Permissions.Menus.Delete";
        public const string Assign = "Permissions.Menus.Assign";
    }

    public static class Actions
    {
        public const string View = "Permissions.Actions.View";
        public const string Create = "Permissions.Actions.Create";
        public const string Edit = "Permissions.Actions.Edit";
        public const string Delete = "Permissions.Actions.Delete";
    }

    public static class Settings
    {
        public const string View = "Permissions.Settings.View";
        public const string Manage = "Permissions.Settings.Manage";
    }

    public static class AuditLogs
    {
        public const string View = "Permissions.AuditLogs.View";
    }

    public static IReadOnlyList<string> GetAllPermissions()
    {
        return new[]
        {
            Users.View, Users.Create, Users.Edit, Users.Delete, Users.ManageRoles,
            Roles.View, Roles.Create, Roles.Edit, Roles.Delete, Roles.ManagePermissions,
            Menus.View, Menus.Create, Menus.Edit, Menus.Delete, Menus.Assign,
            Actions.View, Actions.Create, Actions.Edit, Actions.Delete,
            Settings.View, Settings.Manage,
            AuditLogs.View
        };
    }
}
