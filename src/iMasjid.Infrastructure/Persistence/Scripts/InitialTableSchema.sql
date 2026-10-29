-- ============================================================================
-- SCRIPT: InitialTableSchema.sql
-- DESCRIPTION: Complete DDL Script for PostgreSQL (pgAdmin / psql)
--              Creates Identity & Management (IM_) tables, constraints,
--              indexes, views, functions, and stored procedures.
-- COMPATIBILITY: PostgreSQL 13+, PostgreSQL 14, 15, 16, 17+
-- ============================================================================

-- Ensure pgcrypto extension is available for UUID generation (if using PostgreSQL < 13)
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================================
-- 1. TABLES & CONSTRAINTS
-- ============================================================================

-- 1.1 IM_Users
CREATE TABLE IF NOT EXISTS "IM_Users" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "UserName" VARCHAR(50) NOT NULL,
    "Email" VARCHAR(256) NOT NULL,
    "PhoneNumber" VARCHAR(20) NULL,
    "PasswordHash" TEXT NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsEmailVerified" BOOLEAN NOT NULL DEFAULT FALSE,
    "TwoFactorEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "LastLoginDate" TIMESTAMPTZ NULL,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    "ModifiedOn" TIMESTAMPTZ NULL,
    "ModifiedBy" VARCHAR(100) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedOn" TIMESTAMPTZ NULL,
    "DeletedBy" VARCHAR(100) NULL
);

-- 1.2 IM_Roles
CREATE TABLE IF NOT EXISTS "IM_Roles" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(50) NOT NULL,
    "NormalizedName" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(250) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    "ModifiedOn" TIMESTAMPTZ NULL,
    "ModifiedBy" VARCHAR(100) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedOn" TIMESTAMPTZ NULL,
    "DeletedBy" VARCHAR(100) NULL
);

-- 1.3 IM_Permissions
CREATE TABLE IF NOT EXISTS "IM_Permissions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(100) NOT NULL,
    "Code" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(250) NULL,
    "Module" VARCHAR(50) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    "ModifiedOn" TIMESTAMPTZ NULL,
    "ModifiedBy" VARCHAR(100) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedOn" TIMESTAMPTZ NULL,
    "DeletedBy" VARCHAR(100) NULL
);

-- 1.4 IM_UserRoles
CREATE TABLE IF NOT EXISTS "IM_UserRoles" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "RoleId" UUID NOT NULL,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    CONSTRAINT "FK_IM_UserRoles_Users" FOREIGN KEY ("UserId") REFERENCES "IM_Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_IM_UserRoles_Roles" FOREIGN KEY ("RoleId") REFERENCES "IM_Roles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_IM_UserRoles_User_Role" UNIQUE ("UserId", "RoleId")
);

-- 1.5 IM_RolePermissions
CREATE TABLE IF NOT EXISTS "IM_RolePermissions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "RoleId" UUID NOT NULL,
    "PermissionId" UUID NOT NULL,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    CONSTRAINT "FK_IM_RolePermissions_Roles" FOREIGN KEY ("RoleId") REFERENCES "IM_Roles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_IM_RolePermissions_Permissions" FOREIGN KEY ("PermissionId") REFERENCES "IM_Permissions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_IM_RolePermissions_Role_Permission" UNIQUE ("RoleId", "PermissionId")
);

-- 1.6 IM_RefreshTokens
CREATE TABLE IF NOT EXISTS "IM_RefreshTokens" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "Token" VARCHAR(256) NOT NULL,
    "JwtId" VARCHAR(128) NOT NULL,
    "IsUsed" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsRevoked" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedByIp" VARCHAR(50) NULL,
    "ExpiresOn" TIMESTAMPTZ NOT NULL,
    "RevokedOn" TIMESTAMPTZ NULL,
    "RevokedByIp" VARCHAR(50) NULL,
    "ReplacedByToken" VARCHAR(256) NULL,
    CONSTRAINT "FK_IM_RefreshTokens_Users" FOREIGN KEY ("UserId") REFERENCES "IM_Users" ("Id") ON DELETE CASCADE
);

-- 1.7 IM_UserSessions
CREATE TABLE IF NOT EXISTS "IM_UserSessions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "SessionToken" VARCHAR(256) NOT NULL,
    "DeviceId" VARCHAR(100) NULL,
    "DeviceName" VARCHAR(150) NULL,
    "IpAddress" VARCHAR(50) NULL,
    "UserAgent" VARCHAR(500) NULL,
    "LastActivity" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ExpiresOn" TIMESTAMPTZ NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "RevokedOn" TIMESTAMPTZ NULL,
    CONSTRAINT "FK_IM_UserSessions_Users" FOREIGN KEY ("UserId") REFERENCES "IM_Users" ("Id") ON DELETE CASCADE
);

-- 1.8 IM_LoginHistory
CREATE TABLE IF NOT EXISTS "IM_LoginHistory" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NULL,
    "UserNameOrEmail" VARCHAR(256) NOT NULL,
    "LoginTime" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IpAddress" VARCHAR(50) NULL,
    "UserAgent" VARCHAR(500) NULL,
    "IsSuccess" BOOLEAN NOT NULL,
    "FailureReason" VARCHAR(250) NULL,
    CONSTRAINT "FK_IM_LoginHistory_Users" FOREIGN KEY ("UserId") REFERENCES "IM_Users" ("Id") ON DELETE SET NULL
);

-- 1.9 IM_AuditLogs
CREATE TABLE IF NOT EXISTS "IM_AuditLogs" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" VARCHAR(100) NULL,
    "Action" INT NOT NULL,
    "EntityName" VARCHAR(100) NOT NULL,
    "EntityId" VARCHAR(100) NULL,
    "OldValues" TEXT NULL,
    "NewValues" TEXT NULL,
    "IpAddress" VARCHAR(50) NULL,
    "UserAgent" VARCHAR(500) NULL,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 1.10 IM_ApplicationSettings
CREATE TABLE IF NOT EXISTS "IM_ApplicationSettings" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Key" VARCHAR(100) NOT NULL,
    "Value" TEXT NOT NULL,
    "Description" VARCHAR(250) NULL,
    "Module" VARCHAR(50) NOT NULL DEFAULT 'General',
    "IsEncrypted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    "ModifiedOn" TIMESTAMPTZ NULL,
    "ModifiedBy" VARCHAR(100) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedOn" TIMESTAMPTZ NULL,
    "DeletedBy" VARCHAR(100) NULL
);

-- 1.11 IM_Menus (Hierarchical Master Menu)
CREATE TABLE IF NOT EXISTS "IM_Menus" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Title" VARCHAR(100) NOT NULL,
    "Route" VARCHAR(256) NULL,
    "Icon" VARCHAR(100) NULL,
    "ParentId" UUID NULL,
    "OrderIndex" INT NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    "ModifiedOn" TIMESTAMPTZ NULL,
    "ModifiedBy" VARCHAR(100) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedOn" TIMESTAMPTZ NULL,
    "DeletedBy" VARCHAR(100) NULL,
    CONSTRAINT "FK_IM_Menus_ParentMenu" FOREIGN KEY ("ParentId") REFERENCES "IM_Menus" ("Id")
);

-- 1.12 IM_BusinessActions (Action Master: CREATE, VIEW, EDIT, DELETE, EXPORT, etc.)
CREATE TABLE IF NOT EXISTS "IM_BusinessActions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(100) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(250) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    "ModifiedOn" TIMESTAMPTZ NULL,
    "ModifiedBy" VARCHAR(100) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedOn" TIMESTAMPTZ NULL,
    "DeletedBy" VARCHAR(100) NULL
);

-- 1.13 IM_MenuActions (Available Business Actions for a Menu)
CREATE TABLE IF NOT EXISTS "IM_MenuActions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "MenuId" UUID NOT NULL,
    "ActionId" UUID NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    CONSTRAINT "FK_IM_MenuActions_Menus" FOREIGN KEY ("MenuId") REFERENCES "IM_Menus" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_IM_MenuActions_BusinessActions" FOREIGN KEY ("ActionId") REFERENCES "IM_BusinessActions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_IM_MenuActions_Menu_Action" UNIQUE ("MenuId", "ActionId")
);

-- 1.14 IM_RoleMenus (Menu visibility granted to Roles)
CREATE TABLE IF NOT EXISTS "IM_RoleMenus" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "RoleId" UUID NOT NULL,
    "MenuId" UUID NOT NULL,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    CONSTRAINT "FK_IM_RoleMenus_Roles" FOREIGN KEY ("RoleId") REFERENCES "IM_Roles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_IM_RoleMenus_Menus" FOREIGN KEY ("MenuId") REFERENCES "IM_Menus" ("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_IM_RoleMenus_Role_Menu" UNIQUE ("RoleId", "MenuId")
);

-- 1.15 IM_RoleMenuActions (Business Action permissions granted to a Role under a Menu)
CREATE TABLE IF NOT EXISTS "IM_RoleMenuActions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "RoleMenuId" UUID NOT NULL,
    "MenuActionId" UUID NOT NULL,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    CONSTRAINT "FK_IM_RoleMenuActions_RoleMenus" FOREIGN KEY ("RoleMenuId") REFERENCES "IM_RoleMenus" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_IM_RoleMenuActions_MenuActions" FOREIGN KEY ("MenuActionId") REFERENCES "IM_MenuActions" ("Id") ON DELETE NO ACTION,
    CONSTRAINT "UQ_IM_RoleMenuActions_RoleMenu_Action" UNIQUE ("RoleMenuId", "MenuActionId")
);

-- 1.16 IM_UserOtps (Authentication & Two-Factor OTP Codes)
CREATE TABLE IF NOT EXISTS "IM_UserOtps" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "OtpCode" VARCHAR(10) NOT NULL,
    "Purpose" VARCHAR(50) NOT NULL,
    "ExpiresOn" TIMESTAMPTZ NOT NULL,
    "IsUsed" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedByIp" VARCHAR(50) NULL,
    "UsedOn" TIMESTAMPTZ NULL,
    "AttemptsCount" INT NOT NULL DEFAULT 0,
    "MaxAttempts" INT NOT NULL DEFAULT 5,
    CONSTRAINT "FK_IM_UserOtps_Users" FOREIGN KEY ("UserId") REFERENCES "IM_Users" ("Id") ON DELETE CASCADE
);

-- ============================================================================
-- 2. INDEXES
-- ============================================================================

CREATE UNIQUE INDEX IF NOT EXISTS "IX_IM_Users_Email" ON "IM_Users" ("Email");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_IM_Users_UserName" ON "IM_Users" ("UserName");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_IM_Roles_NormalizedName" ON "IM_Roles" ("NormalizedName");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_IM_Permissions_Code" ON "IM_Permissions" ("Code");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_IM_RefreshTokens_Token" ON "IM_RefreshTokens" ("Token");
CREATE INDEX IF NOT EXISTS "IX_IM_RefreshTokens_UserId" ON "IM_RefreshTokens" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_IM_UserSessions_UserId" ON "IM_UserSessions" ("UserId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_IM_UserSessions_Token" ON "IM_UserSessions" ("SessionToken");
CREATE INDEX IF NOT EXISTS "IX_IM_LoginHistory_UserId" ON "IM_LoginHistory" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_IM_LoginHistory_Time" ON "IM_LoginHistory" ("LoginTime");
CREATE INDEX IF NOT EXISTS "IX_IM_AuditLogs_UserId" ON "IM_AuditLogs" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_IM_AuditLogs_EntityName" ON "IM_AuditLogs" ("EntityName");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_IM_ApplicationSettings_Key" ON "IM_ApplicationSettings" ("Key");
CREATE INDEX IF NOT EXISTS "IX_IM_Menus_ParentId" ON "IM_Menus" ("ParentId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_IM_BusinessActions_Code" ON "IM_BusinessActions" ("Code");
CREATE INDEX IF NOT EXISTS "IX_IM_MenuActions_MenuId" ON "IM_MenuActions" ("MenuId");
CREATE INDEX IF NOT EXISTS "IX_IM_RoleMenus_RoleId" ON "IM_RoleMenus" ("RoleId");
CREATE INDEX IF NOT EXISTS "IX_IM_RoleMenuActions_RoleMenuId" ON "IM_RoleMenuActions" ("RoleMenuId");
CREATE INDEX IF NOT EXISTS "IX_IM_UserOtps_UserId_Purpose_IsUsed" ON "IM_UserOtps" ("UserId", "Purpose", "IsUsed");
CREATE INDEX IF NOT EXISTS "IX_IM_UserOtps_ExpiresOn" ON "IM_UserOtps" ("ExpiresOn");

-- ============================================================================
-- 3. VIEWS
-- ============================================================================

CREATE OR REPLACE VIEW "vw_UserEffectivePermissions"
AS
SELECT 
    u."Id" AS "UserId",
    u."UserName",
    u."Email",
    p."Code" AS "PermissionCode",
    p."Name" AS "PermissionName",
    p."Module" AS "PermissionModule",
    'Role: ' || r."Name" AS "Source"
FROM "IM_Users" u
INNER JOIN "IM_UserRoles" ur ON u."Id" = ur."UserId"
INNER JOIN "IM_Roles" r ON ur."RoleId" = r."Id"
INNER JOIN "IM_RolePermissions" rp ON r."Id" = rp."RoleId"
INNER JOIN "IM_Permissions" p ON rp."PermissionId" = p."Id"
WHERE u."IsDeleted" = FALSE AND r."IsDeleted" = FALSE AND r."IsActive" = TRUE AND p."IsDeleted" = FALSE AND p."IsActive" = TRUE;

-- ============================================================================
-- 4. FUNCTIONS
-- ============================================================================

CREATE OR REPLACE FUNCTION "fn_HasPermission"(
    p_user_id UUID,
    p_permission_code VARCHAR(100)
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1 
        FROM "vw_UserEffectivePermissions"
        WHERE "UserId" = p_user_id AND "PermissionCode" = p_permission_code
    );
END;
$$;

-- ============================================================================
-- 5. STORED PROCEDURES / FUNCTIONS
-- ============================================================================

CREATE OR REPLACE FUNCTION "sp_PurgeExpiredRefreshTokens"(
    p_days_old INT DEFAULT 30
)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
    v_deleted_count INT;
BEGIN
    DELETE FROM "IM_RefreshTokens"
    WHERE ("IsRevoked" = TRUE OR "IsUsed" = TRUE OR "ExpiresOn" < CURRENT_TIMESTAMP)
      AND "CreatedOn" < (CURRENT_TIMESTAMP - (p_days_old || ' days')::INTERVAL);

    GET DIAGNOSTICS v_deleted_count = ROW_COUNT;
    RETURN v_deleted_count;
END;
$$;

-- ============================================================================
-- 6. IM_Locales (Supported Locales & Languages)
-- ============================================================================

CREATE TABLE IF NOT EXISTS "IM_Locales" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DisplayOrder" INT NOT NULL DEFAULT 1,
    "CountryName" VARCHAR(100) NOT NULL,
    "PhoneCode" VARCHAR(20) NOT NULL,
    "LanguageName" VARCHAR(100) NOT NULL,
    "CultureCode" VARCHAR(20) NOT NULL,
    "CurrencySymbol" VARCHAR(20) NOT NULL,
    "CurrencyName" VARCHAR(100) NOT NULL,
    "CurrencyCode" VARCHAR(10) NOT NULL,
    "Direction" VARCHAR(10) NOT NULL DEFAULT 'LTR',
    "FlagIconUrl" VARCHAR(255) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    "ModifiedOn" TIMESTAMPTZ NULL,
    "ModifiedBy" VARCHAR(100) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedOn" TIMESTAMPTZ NULL,
    "DeletedBy" VARCHAR(100) NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_IM_Locales_CultureCode" ON "IM_Locales" ("CultureCode");
CREATE INDEX IF NOT EXISTS "IX_IM_Locales_DisplayOrder" ON "IM_Locales" ("DisplayOrder");
CREATE INDEX IF NOT EXISTS "IX_IM_Locales_IsActive" ON "IM_Locales" ("IsActive");

