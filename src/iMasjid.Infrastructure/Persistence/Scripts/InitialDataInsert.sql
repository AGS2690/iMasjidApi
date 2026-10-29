-- ============================================================================
-- SCRIPT: InitialDataInsert.sql
-- DESCRIPTION: Initial seed data for PostgreSQL (pgAdmin / psql)
--              Populates system roles, permissions, role-permissions,
--              default Super Admin user, application settings, business actions,
--              and dynamic menus in Identity & Management (IM_) tables.
-- NOTE: Primary keys are generated automatically by table DEFAULT (gen_random_uuid()).
--       Foreign key relationships are dynamically resolved via subqueries.
-- COMPATIBILITY: PostgreSQL 13+, PostgreSQL 14, 15, 16, 17+
-- ============================================================================

-- ============================================================================
-- 1. SYSTEM ROLES (Masjid Domain Specific Roles)
-- ============================================================================

INSERT INTO "IM_Roles" ("Name", "NormalizedName", "Description", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
VALUES 
    ('Super Admin', 'SUPER ADMIN', 'Super Administrator with unrestricted access', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Ministry Admin', 'MINISTRY ADMIN', 'Ministry level administrator', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Super Mosque Admin', 'SUPER MOSQUE ADMIN', 'Super administrator across multiple mosques', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Mosque Admin', 'MOSQUE ADMIN', 'Administrator for specific mosque operations', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Mosque Assistant', 'MOSQUE ASSISTANT', 'Assistant staff supporting mosque administration', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Imam', 'IMAM', 'Mosque religious leader and prayer coordinator', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Member', 'MEMBER', 'Registered community mosque member', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Office Bearer', 'OFFICE BEARER', 'Mosque committee or board office bearer', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Support Team', 'SUPPORT TEAM', 'Technical and operational support team', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Developer', 'DEVELOPER', 'System and platform developer', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Vendor', 'VENDOR', 'External service provider or supplier', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Technician', 'TECHNICIAN', 'Maintenance and hardware technician', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Accountant', 'ACCOUNTANT', 'Financial and accounting administrator', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE)
ON CONFLICT ("NormalizedName") DO NOTHING;

-- ============================================================================
-- 2. SYSTEM PERMISSIONS
-- ============================================================================

INSERT INTO "IM_Permissions" ("Name", "Code", "Description", "Module", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
VALUES 
    -- Users Permissions
    ('Users View', 'Permissions.Users.View', 'Allows viewing users', 'Users', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Users Create', 'Permissions.Users.Create', 'Allows creating users', 'Users', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Users Edit', 'Permissions.Users.Edit', 'Allows editing users', 'Users', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Users Delete', 'Permissions.Users.Delete', 'Allows deleting users', 'Users', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Users Manage Roles', 'Permissions.Users.ManageRoles', 'Allows assigning roles to users', 'Users', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),

    -- Roles Permissions
    ('Roles View', 'Permissions.Roles.View', 'Allows viewing roles', 'Roles', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Roles Create', 'Permissions.Roles.Create', 'Allows creating roles', 'Roles', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Roles Edit', 'Permissions.Roles.Edit', 'Allows editing roles', 'Roles', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Roles Delete', 'Permissions.Roles.Delete', 'Allows deleting roles', 'Roles', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Roles Manage Permissions', 'Permissions.Roles.ManagePermissions', 'Allows assigning permissions to roles', 'Roles', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),

    -- Menus Permissions
    ('Menus View', 'Permissions.Menus.View', 'Allows viewing menus', 'Menus', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Menus Create', 'Permissions.Menus.Create', 'Allows creating menus', 'Menus', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Menus Edit', 'Permissions.Menus.Edit', 'Allows editing menus', 'Menus', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Menus Delete', 'Permissions.Menus.Delete', 'Allows deleting menus', 'Menus', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Menus Assign', 'Permissions.Menus.Assign', 'Allows assigning menus and actions to roles', 'Menus', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),

    -- Actions Permissions
    ('Actions View', 'Permissions.Actions.View', 'Allows viewing business actions', 'Actions', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Actions Create', 'Permissions.Actions.Create', 'Allows creating business actions', 'Actions', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Actions Edit', 'Permissions.Actions.Edit', 'Allows editing business actions', 'Actions', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Actions Delete', 'Permissions.Actions.Delete', 'Allows deleting business actions', 'Actions', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),

    -- Settings Permissions
    ('Settings View', 'Permissions.Settings.View', 'Allows viewing application settings', 'Settings', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Settings Manage', 'Permissions.Settings.Manage', 'Allows managing application settings', 'Settings', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),

    -- AuditLogs Permissions
    ('AuditLogs View', 'Permissions.AuditLogs.View', 'Allows viewing audit logs', 'AuditLogs', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE)
ON CONFLICT ("Code") DO NOTHING;

-- ============================================================================
-- 3. ROLE-PERMISSIONS ASSIGNMENTS
-- ============================================================================

-- 3.1 Super Admin receives ALL permissions
INSERT INTO "IM_RolePermissions" ("RoleId", "PermissionId", "CreatedOn", "CreatedBy")
SELECT r."Id", p."Id", CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_Roles" r
CROSS JOIN "IM_Permissions" p
WHERE r."NormalizedName" = 'SUPER ADMIN'
ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;

-- 3.2 Ministry Admin & Mosque Admins receive Management Permissions
INSERT INTO "IM_RolePermissions" ("RoleId", "PermissionId", "CreatedOn", "CreatedBy")
SELECT r."Id", p."Id", CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_Roles" r
CROSS JOIN "IM_Permissions" p
WHERE r."NormalizedName" IN ('MINISTRY ADMIN', 'SUPER MOSQUE ADMIN', 'MOSQUE ADMIN')
  AND p."Code" IN (
      'Permissions.Users.View', 'Permissions.Users.Create', 'Permissions.Users.Edit', 'Permissions.Users.ManageRoles',
      'Permissions.Roles.View', 'Permissions.Menus.View', 'Permissions.Menus.Assign', 'Permissions.Actions.View',
      'Permissions.Settings.View', 'Permissions.AuditLogs.View'
  )
ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;

-- 3.3 Accountant receives financial, user, setting, and audit permissions
INSERT INTO "IM_RolePermissions" ("RoleId", "PermissionId", "CreatedOn", "CreatedBy")
SELECT r."Id", p."Id", CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_Roles" r
CROSS JOIN "IM_Permissions" p
WHERE r."NormalizedName" = 'ACCOUNTANT'
  AND p."Code" IN ('Permissions.Users.View', 'Permissions.Settings.View', 'Permissions.AuditLogs.View')
ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;

-- 3.4 Members, Imams, and Staff receive read-only view permissions
INSERT INTO "IM_RolePermissions" ("RoleId", "PermissionId", "CreatedOn", "CreatedBy")
SELECT r."Id", p."Id", CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_Roles" r
CROSS JOIN "IM_Permissions" p
WHERE r."NormalizedName" IN ('MEMBER', 'IMAM', 'OFFICE BEARER', 'MOSQUE ASSISTANT', 'SUPPORT TEAM', 'DEVELOPER', 'VENDOR', 'TECHNICIAN')
  AND p."Code" IN ('Permissions.Users.View', 'Permissions.Settings.View')
ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;

-- ============================================================================
-- 4. DEFAULT SUPERADMIN USER (Credentials: superadmin@imasjid.com / SuperAdmin123!)
-- ============================================================================

INSERT INTO "IM_Users" (
    "FirstName", "LastName", "UserName", "Email", "PhoneNumber", "PasswordHash",
    "IsActive", "IsEmailVerified", "TwoFactorEnabled", "CreatedOn", "CreatedBy", "IsDeleted"
)
VALUES (
    'System', 'SuperAdmin', 'superadmin', 'superadmin@imasjid.com',
    '+1234567890',
    'e4ObsY5E71mtUgt1zakDQg==:vEo/bazPo+sF30Jr/kB94LiE2eAdFAdvnsFRGwRy0d4=:100000:SHA512',
    TRUE, TRUE, FALSE, CURRENT_TIMESTAMP, 'SeedScript', FALSE
)
ON CONFLICT ("Email") DO NOTHING;

-- Assign Super Admin Role to SuperAdmin User
INSERT INTO "IM_UserRoles" ("UserId", "RoleId", "CreatedOn", "CreatedBy")
SELECT u."Id", r."Id", CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_Users" u
CROSS JOIN "IM_Roles" r
WHERE u."Email" = 'superadmin@imasjid.com'
  AND r."NormalizedName" = 'SUPER ADMIN'
ON CONFLICT ("UserId", "RoleId") DO NOTHING;

-- ============================================================================
-- 5. DEFAULT APPLICATION SETTINGS
-- ============================================================================

INSERT INTO "IM_ApplicationSettings" ("Key", "Value", "Description", "Module", "IsEncrypted", "CreatedOn", "CreatedBy", "IsDeleted")
VALUES 
    ('Security.PasswordExpiryDays', '90', 'Days after which user password expires', 'Security', FALSE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('System.AppName', 'iMasjid Enterprise API', 'Display name of the application', 'General', FALSE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('System.EnableAuditLogs', 'true', 'Flag indicating whether audit logging is enabled', 'Auditing', FALSE, CURRENT_TIMESTAMP, 'SeedScript', FALSE)
ON CONFLICT ("Key") DO NOTHING;

-- ============================================================================
-- 6. BUSINESS ACTIONS MASTER
-- ============================================================================

INSERT INTO "IM_BusinessActions" ("Name", "Code", "Description", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
VALUES 
    ('View / Read', 'VIEW', 'Allows viewing data and accessing screens', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Create / Add', 'CREATE', 'Allows creating new records', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Edit / Update', 'EDIT', 'Allows editing and modifying existing records', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Delete / Remove', 'DELETE', 'Allows deleting records', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Export Data', 'EXPORT', 'Allows exporting reports, Excel, or PDF data', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE),
    ('Import Data', 'IMPORT', 'Allows bulk data upload and importing', TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE)
ON CONFLICT ("Code") DO NOTHING;

-- ============================================================================
-- 7. MENUS & SUBMENUS HIERARCHY
-- ============================================================================

-- 7.1 Root Menus
INSERT INTO "IM_Menus" ("Title", "Route", "Icon", "ParentId", "OrderIndex", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
SELECT 'Dashboard', '/dashboard', 'dashboard', NULL, 1, TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE
WHERE NOT EXISTS (SELECT 1 FROM "IM_Menus" WHERE "Title" = 'Dashboard' AND "ParentId" IS NULL);

INSERT INTO "IM_Menus" ("Title", "Route", "Icon", "ParentId", "OrderIndex", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
SELECT 'Administration', NULL, 'security', NULL, 2, TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE
WHERE NOT EXISTS (SELECT 1 FROM "IM_Menus" WHERE "Title" = 'Administration' AND "ParentId" IS NULL);

INSERT INTO "IM_Menus" ("Title", "Route", "Icon", "ParentId", "OrderIndex", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
SELECT 'Settings & System', NULL, 'settings', NULL, 3, TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE
WHERE NOT EXISTS (SELECT 1 FROM "IM_Menus" WHERE "Title" = 'Settings & System' AND "ParentId" IS NULL);

-- 7.2 Submenus under 'Administration'
INSERT INTO "IM_Menus" ("Title", "Route", "Icon", "ParentId", "OrderIndex", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
SELECT 'User Management', '/admin/users', 'people', m."Id", 1, TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE
FROM "IM_Menus" m
WHERE m."Title" = 'Administration' AND m."ParentId" IS NULL
  AND NOT EXISTS (SELECT 1 FROM "IM_Menus" sub WHERE sub."Title" = 'User Management' AND sub."ParentId" = m."Id");

INSERT INTO "IM_Menus" ("Title", "Route", "Icon", "ParentId", "OrderIndex", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
SELECT 'Roles & Permissions', '/admin/roles', 'vpn_key', m."Id", 2, TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE
FROM "IM_Menus" m
WHERE m."Title" = 'Administration' AND m."ParentId" IS NULL
  AND NOT EXISTS (SELECT 1 FROM "IM_Menus" sub WHERE sub."Title" = 'Roles & Permissions' AND sub."ParentId" = m."Id");

INSERT INTO "IM_Menus" ("Title", "Route", "Icon", "ParentId", "OrderIndex", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
SELECT 'Menu Setup', '/admin/menus', 'menu', m."Id", 3, TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE
FROM "IM_Menus" m
WHERE m."Title" = 'Administration' AND m."ParentId" IS NULL
  AND NOT EXISTS (SELECT 1 FROM "IM_Menus" sub WHERE sub."Title" = 'Menu Setup' AND sub."ParentId" = m."Id");

INSERT INTO "IM_Menus" ("Title", "Route", "Icon", "ParentId", "OrderIndex", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
SELECT 'Action Master', '/admin/actions', 'touch_app', m."Id", 4, TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE
FROM "IM_Menus" m
WHERE m."Title" = 'Administration' AND m."ParentId" IS NULL
  AND NOT EXISTS (SELECT 1 FROM "IM_Menus" sub WHERE sub."Title" = 'Action Master' AND sub."ParentId" = m."Id");

-- 7.3 Submenus under 'Settings & System'
INSERT INTO "IM_Menus" ("Title", "Route", "Icon", "ParentId", "OrderIndex", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
SELECT 'Application Settings', '/system/settings', 'tune', m."Id", 1, TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE
FROM "IM_Menus" m
WHERE m."Title" = 'Settings & System' AND m."ParentId" IS NULL
  AND NOT EXISTS (SELECT 1 FROM "IM_Menus" sub WHERE sub."Title" = 'Application Settings' AND sub."ParentId" = m."Id");

INSERT INTO "IM_Menus" ("Title", "Route", "Icon", "ParentId", "OrderIndex", "IsActive", "CreatedOn", "CreatedBy", "IsDeleted")
SELECT 'Audit Logs', '/system/audit-logs', 'history', m."Id", 2, TRUE, CURRENT_TIMESTAMP, 'SeedScript', FALSE
FROM "IM_Menus" m
WHERE m."Title" = 'Settings & System' AND m."ParentId" IS NULL
  AND NOT EXISTS (SELECT 1 FROM "IM_Menus" sub WHERE sub."Title" = 'Audit Logs' AND sub."ParentId" = m."Id");

-- ============================================================================
-- 8. MENU-ACTIONS MAPPING (Attach business actions to menus)
-- ============================================================================

INSERT INTO "IM_MenuActions" ("MenuId", "ActionId", "IsActive", "CreatedOn", "CreatedBy")
SELECT m."Id", a."Id", TRUE, CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_Menus" m
CROSS JOIN "IM_BusinessActions" a
WHERE m."ParentId" IS NOT NULL
  AND a."Code" IN ('VIEW', 'CREATE', 'EDIT', 'DELETE', 'EXPORT')
ON CONFLICT ("MenuId", "ActionId") DO NOTHING;

-- Dashboard Menu only has VIEW action
INSERT INTO "IM_MenuActions" ("MenuId", "ActionId", "IsActive", "CreatedOn", "CreatedBy")
SELECT m."Id", a."Id", TRUE, CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_Menus" m
CROSS JOIN "IM_BusinessActions" a
WHERE m."Route" = '/dashboard'
  AND a."Code" = 'VIEW'
ON CONFLICT ("MenuId", "ActionId") DO NOTHING;

-- ============================================================================
-- 9. ROLE-MENU & ROLE-MENU-ACTION ASSIGNMENTS
-- ============================================================================

-- 9.1 Super Admin: Assign ALL Menus
INSERT INTO "IM_RoleMenus" ("RoleId", "MenuId", "CreatedOn", "CreatedBy")
SELECT r."Id", m."Id", CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_Roles" r
CROSS JOIN "IM_Menus" m
WHERE r."NormalizedName" = 'SUPER ADMIN'
ON CONFLICT ("RoleId", "MenuId") DO NOTHING;

-- 9.2 Super Admin: Assign ALL Menu Actions
INSERT INTO "IM_RoleMenuActions" ("RoleMenuId", "MenuActionId", "CreatedOn", "CreatedBy")
SELECT rm."Id", ma."Id", CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_RoleMenus" rm
INNER JOIN "IM_Roles" r ON rm."RoleId" = r."Id"
INNER JOIN "IM_MenuActions" ma ON rm."MenuId" = ma."MenuId"
WHERE r."NormalizedName" = 'SUPER ADMIN'
ON CONFLICT ("RoleMenuId", "MenuActionId") DO NOTHING;

-- 9.3 Admins (Ministry Admin, Super Mosque Admin, Mosque Admin): Assign Dashboard, Admin, and Settings Menus
INSERT INTO "IM_RoleMenus" ("RoleId", "MenuId", "CreatedOn", "CreatedBy")
SELECT r."Id", m."Id", CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_Roles" r
CROSS JOIN "IM_Menus" m
WHERE r."NormalizedName" IN ('MINISTRY ADMIN', 'SUPER MOSQUE ADMIN', 'MOSQUE ADMIN')
  AND m."Title" IN (
      'Dashboard', 'Administration', 'User Management', 'Roles & Permissions',
      'Menu Setup', 'Action Master', 'Settings & System', 'Application Settings', 'Audit Logs'
  )
ON CONFLICT ("RoleId", "MenuId") DO NOTHING;

-- 9.4 Admins: Assign VIEW, CREATE, EDIT, EXPORT actions
INSERT INTO "IM_RoleMenuActions" ("RoleMenuId", "MenuActionId", "CreatedOn", "CreatedBy")
SELECT rm."Id", ma."Id", CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_RoleMenus" rm
INNER JOIN "IM_Roles" r ON rm."RoleId" = r."Id"
INNER JOIN "IM_MenuActions" ma ON rm."MenuId" = ma."MenuId"
INNER JOIN "IM_BusinessActions" a ON ma."ActionId" = a."Id"
WHERE r."NormalizedName" IN ('MINISTRY ADMIN', 'SUPER MOSQUE ADMIN', 'MOSQUE ADMIN')
  AND a."Code" IN ('VIEW', 'CREATE', 'EDIT', 'EXPORT')
ON CONFLICT ("RoleMenuId", "MenuActionId") DO NOTHING;

-- 9.5 Members, Imams, and Staff: Assign Dashboard with VIEW action
INSERT INTO "IM_RoleMenus" ("RoleId", "MenuId", "CreatedOn", "CreatedBy")
SELECT r."Id", m."Id", CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_Roles" r
CROSS JOIN "IM_Menus" m
WHERE r."NormalizedName" IN ('MEMBER', 'IMAM', 'OFFICE BEARER', 'MOSQUE ASSISTANT', 'SUPPORT TEAM', 'DEVELOPER', 'VENDOR', 'TECHNICIAN', 'ACCOUNTANT')
  AND m."Title" = 'Dashboard'
ON CONFLICT ("RoleId", "MenuId") DO NOTHING;

INSERT INTO "IM_RoleMenuActions" ("RoleMenuId", "MenuActionId", "CreatedOn", "CreatedBy")
SELECT rm."Id", ma."Id", CURRENT_TIMESTAMP, 'SeedScript'
FROM "IM_RoleMenus" rm
INNER JOIN "IM_Roles" r ON rm."RoleId" = r."Id"
INNER JOIN "IM_MenuActions" ma ON rm."MenuId" = ma."MenuId"
INNER JOIN "IM_BusinessActions" a ON ma."ActionId" = a."Id"
WHERE r."NormalizedName" IN ('MEMBER', 'IMAM', 'OFFICE BEARER', 'MOSQUE ASSISTANT', 'SUPPORT TEAM', 'DEVELOPER', 'VENDOR', 'TECHNICIAN', 'ACCOUNTANT')
  AND a."Code" = 'VIEW'
ON CONFLICT ("RoleMenuId", "MenuActionId") DO NOTHING;

-- ============================================================================
-- 10. SUPPORTED LOCALES & LANGUAGES
-- ============================================================================

INSERT INTO "IM_Locales" (
    "Id",
    "DisplayOrder",
    "CountryName",
    "PhoneCode",
    "LanguageName",
    "CultureCode",
    "CurrencySymbol",
    "CurrencyName",
    "CurrencyCode",
    "Direction",
    "FlagIconUrl",
    "IsActive",
    "CreatedBy"
)
VALUES
(
    '9d0e2b08-2365-42bb-b2b8-aaffa57ec2fe'::UUID,
    1,
    'Indonesia',
    '+62',
    'Bahasa',
    'id-ID',
    'Rp',
    'Indonesian Rupiah',
    'IDR',
    'LTR',
    'assets/flags/BI.png',
    TRUE,
    'SeedScript'
),
(
    '7185939a-a861-4435-b04a-75a004831027'::UUID,
    2,
    'Saudi Arabia',
    '+966',
    'عربي',
    'ar-SA',
    '﷼',
    'Saudi Riyal',
    'SAR',
    'RTL',
    'assets/flags/AR.svg',
    TRUE,
    'SeedScript'
),
(
    '5147bd8b-79a2-4e72-95ca-4a33fae51d6d'::UUID,
    3,
    'United States',
    '+1',
    'English (US)',
    'en-US',
    '$',
    'US Dollar',
    'USD',
    'LTR',
    'assets/flags/EN.svg',
    TRUE,
    'SeedScript'
),
(
    '0cb80eed-e33f-4067-a2b4-21dd38782367'::UUID,
    4,
    'India',
    '+91',
    'English (IN)',
    'en-IN',
    '₹',
    'Indian Rupee',
    'INR',
    'LTR',
    'assets/flags/IN.svg',
    FALSE,
    'SeedScript'
)
ON CONFLICT ("Id") DO UPDATE SET
    "DisplayOrder"   = EXCLUDED."DisplayOrder",
    "CountryName"    = EXCLUDED."CountryName",
    "PhoneCode"      = EXCLUDED."PhoneCode",
    "LanguageName"   = EXCLUDED."LanguageName",
    "CultureCode"    = EXCLUDED."CultureCode",
    "CurrencySymbol" = EXCLUDED."CurrencySymbol",
    "CurrencyName"   = EXCLUDED."CurrencyName",
    "CurrencyCode"   = EXCLUDED."CurrencyCode",
    "Direction"      = EXCLUDED."Direction",
    "FlagIconUrl"    = EXCLUDED."FlagIconUrl",
    "IsActive"       = EXCLUDED."IsActive",
    "ModifiedOn"     = CURRENT_TIMESTAMP,
    "ModifiedBy"     = 'SeedScript';

