-- ============================================================================
-- SCRIPT: CreateTranslationsTable.sql
-- DESCRIPTION: DDL and initial translation seeds for dynamic entities:
--              IM_Menus, IM_BusinessActions, and IM_Permissions.
-- DATABASE: PostgreSQL 13+
-- ============================================================================

-- 1. Create IM_Translations Table
CREATE TABLE IF NOT EXISTS "IM_Translations" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "EntityType" VARCHAR(50) NOT NULL,   -- 'Menu', 'BusinessAction', 'Permission'
    "EntityId" UUID NOT NULL,            -- Id of Menu, BusinessAction, or Permission
    "CultureCode" VARCHAR(20) NOT NULL,  -- 'id-ID', 'ar-SA', 'en-US'
    "Name" VARCHAR(250) NOT NULL,        -- Translated Title / Name
    "Description" TEXT NULL,             -- Translated Description
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    "ModifiedOn" TIMESTAMPTZ NULL,
    "ModifiedBy" VARCHAR(100) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedOn" TIMESTAMPTZ NULL,
    "DeletedBy" VARCHAR(100) NULL,
    CONSTRAINT "UQ_IM_Translations_Entity_Culture" UNIQUE ("EntityType", "EntityId", "CultureCode")
);

-- 2. Indexes
CREATE INDEX IF NOT EXISTS "IX_IM_Translations_Lookup" ON "IM_Translations" ("EntityType", "CultureCode");
CREATE INDEX IF NOT EXISTS "IX_IM_Translations_EntityId" ON "IM_Translations" ("EntityId");

-- ============================================================================
-- 3. SEED TRANSLATIONS: IM_Menus
-- ============================================================================

-- 3.1 Bahasa Indonesia (id-ID) Menus
INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'id-ID', 'Dasbor', 'Ringkasan operasional dan statistik', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/dashboard'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'id-ID', 'Administrasi', 'Menu administrasi dan manajemen peran', 'SeedScript'
FROM "IM_Menus" m WHERE m."Title" = 'Administration' AND m."ParentId" IS NULL
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'id-ID', 'Sistem & Pengaturan', 'Konfigurasi sistem dan log audit', 'SeedScript'
FROM "IM_Menus" m WHERE m."Title" = 'Settings & System' AND m."ParentId" IS NULL
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'id-ID', 'Manajemen Pengguna', 'Kelola akun pengguna, peran, dan status', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/admin/users'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'id-ID', 'Peran & Izin Perizinan', 'Atur hak akses peran dan matriks izin', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/admin/roles'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'id-ID', 'Pengaturan Menu', 'Kelola hierarki menu navigasi dan perizinan', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/admin/menus'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'id-ID', 'Aksi Bisnis', 'Master aksi operasional CRUD dan ekspor', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/admin/actions'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'id-ID', 'Pengaturan Aplikasi', 'Konfigurasi parameter dan kebijakan sistem', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/system/settings'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'id-ID', 'Log Audit', 'Riwayat aktivitas dan catatan perubahan data', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/system/audit-logs'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

-- 3.2 Arabic (ar-SA) Menus
INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'ar-SA', 'لوحة القيادة', 'ملخص العمليات والإحصائيات العامة', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/dashboard'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'ar-SA', 'الإدارة', 'إدارة النظام والأدوار والصلاحيات', 'SeedScript'
FROM "IM_Menus" m WHERE m."Title" = 'Administration' AND m."ParentId" IS NULL
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'ar-SA', 'النظام والإعدادات', 'إعدادات النظام وسجلات التدقيق', 'SeedScript'
FROM "IM_Menus" m WHERE m."Title" = 'Settings & System' AND m."ParentId" IS NULL
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'ar-SA', 'إدارة المستخدمين', 'إدارة حسابات المستخدمين وحالاتهم', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/admin/users'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'ar-SA', 'الأدوار والصلاحيات', 'تحديد أدوار المستخدمين ومصفوفة الأذونات', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/admin/roles'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'ar-SA', 'إعداد القوائم', 'تنظيم هيكل القوائم وربط العمليات', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/admin/menus'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'ar-SA', 'إجراءات الأعمال', 'إدارة العمليات الوظيفية والتصدير', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/admin/actions'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'ar-SA', 'إعدادات التطبيق', 'تكوين المعلمات وسياسات الأمان', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/system/settings'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Menu', m."Id", 'ar-SA', 'سجلات التدقيق', 'تاريخ العمليات والتغييرات على النظام', 'SeedScript'
FROM "IM_Menus" m WHERE m."Route" = '/system/audit-logs'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

-- ============================================================================
-- 4. SEED TRANSLATIONS: IM_BusinessActions
-- ============================================================================

-- 4.1 Bahasa Indonesia (id-ID) Actions
INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'id-ID', 'Lihat / Baca', 'Mengizinkan melihat data dan mengakses layar antarmuka', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'VIEW'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'id-ID', 'Tambah Baru', 'Mengizinkan membuat data dan entri baru', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'CREATE'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'id-ID', 'Ubah / Edit', 'Mengizinkan mengubah dan memperbarui data yang ada', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'EDIT'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'id-ID', 'Hapus Data', 'Mengizinkan menghapus data dari sistem', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'DELETE'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'id-ID', 'Ekspor Data', 'Mengizinkan mengunduh laporan, Excel, dan dokumen PDF', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'EXPORT'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'id-ID', 'Impor Data', 'Mengizinkan unggah dan impor data massal', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'IMPORT'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

-- 4.2 Arabic (ar-SA) Actions
INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'ar-SA', 'عرض / قراءة', 'يسمح بعرض السجلات والوصول إلى الشاشات', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'VIEW'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'ar-SA', 'إضافة جديد', 'يسمح بإنشاء سجلات وبيانات جديدة', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'CREATE'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'ar-SA', 'تعديل / تحديث', 'يسمح بتعديل وتحديث البيانات الموجودة', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'EDIT'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'ar-SA', 'حذف السجل', 'يسمح بحذف السجلات من النظام', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'DELETE'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'ar-SA', 'تصدير البيانات', 'يسمح بتصدير التقارير وجداول Excel وملفات PDF', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'EXPORT'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'BusinessAction', a."Id", 'ar-SA', 'استيراد البيانات', 'يسمح برفع واستيراد البيانات بشكل مجمع', 'SeedScript'
FROM "IM_BusinessActions" a WHERE a."Code" = 'IMPORT'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

-- ============================================================================
-- 5. SEED TRANSLATIONS: IM_Permissions
-- ============================================================================

-- 5.1 Bahasa Indonesia (id-ID) Permissions
INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Lihat Pengguna', 'Mengizinkan melihat daftar pengguna', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Users.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Tambah Pengguna', 'Mengizinkan membuat akun pengguna baru', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Users.Create'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Ubah Pengguna', 'Mengizinkan memperbarui data akun pengguna', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Users.Edit'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Hapus Pengguna', 'Mengizinkan menghapus akun pengguna', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Users.Delete'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Kelola Peran Pengguna', 'Mengizinkan menetapkan peran kepada pengguna', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Users.ManageRoles'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Lihat Peran', 'Mengizinkan melihat daftar peran sistem', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Roles.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Tambah Peran', 'Mengizinkan membuat peran baru', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Roles.Create'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Ubah Peran', 'Mengizinkan memperbarui nama dan keterangan peran', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Roles.Edit'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Hapus Peran', 'Mengizinkan menghapus peran dari sistem', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Roles.Delete'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Kelola Izin Peran', 'Mengizinkan menetapkan izin ke peran pengguna', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Roles.ManagePermissions'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Lihat Menu', 'Mengizinkan melihat daftar menu navigasi', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Menus.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Tambah Menu', 'Mengizinkan menambah menu navigasi baru', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Menus.Create'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Ubah Menu', 'Mengizinkan mengubah rute dan urutan menu', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Menus.Edit'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Hapus Menu', 'Mengizinkan menghapus menu navigasi', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Menus.Delete'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Tetapkan Menu Peran', 'Mengizinkan menetapkan menu dan aksi ke peran', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Menus.Assign'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Lihat Aksi', 'Mengizinkan melihat daftar aksi bisnis', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Actions.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Tambah Aksi', 'Mengizinkan membuat aksi bisnis baru', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Actions.Create'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Ubah Aksi', 'Mengizinkan mengubah kode dan nama aksi bisnis', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Actions.Edit'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Hapus Aksi', 'Mengizinkan menghapus aksi bisnis', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Actions.Delete'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Lihat Pengaturan', 'Mengizinkan melihat pengaturan sistem', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Settings.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Kelola Pengaturan', 'Mengizinkan memperbarui nilai pengaturan aplikasi', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Settings.Manage'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'id-ID', 'Lihat Log Audit', 'Mengizinkan memeriksa catatan audit aktivitas', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.AuditLogs.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

-- 5.2 Arabic (ar-SA) Permissions
INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'عرض المستخدمين', 'يسمح بعرض قائمة المستخدمين', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Users.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'إضافة مستخدم', 'يسمح بإنشاء حسابات مستخدمين جديدة', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Users.Create'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'تعديل مستخدم', 'يسمح بتعديل بيانات حساب المستخدم', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Users.Edit'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'حذف مستخدم', 'يسمح بحذف حساب المستخدم من النظام', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Users.Delete'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'إدارة أدوار المستخدم', 'يسمح بتعيين الأدوار للمستخدمين', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Users.ManageRoles'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'عرض الأدوار', 'يسمح بعرض قائمة الأدوار في النظام', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Roles.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'إضافة دور', 'يسمح بإنشاء أدوار جديدة في النظام', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Roles.Create'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'تعديل دور', 'يسمح بتحديث بيانات ووصف الأدوار', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Roles.Edit'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'حذف دور', 'يسمح بحذف الأدوار من النظام', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Roles.Delete'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'إدارة صلاحيات الدور', 'يسمح بتعيين الصلاحيات لأدوار النظام', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Roles.ManagePermissions'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'عرض القوائم', 'يسمح بعرض قوائم التنقل', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Menus.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'إضافة قائمة', 'يسمح بإنشاء قوائم تنقل جديدة', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Menus.Create'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'تعديل قائمة', 'يسمح بتعديل مسارات وعناوين القوائم', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Menus.Edit'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'حذف قائمة', 'يسمح بحذف القوائم', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Menus.Delete'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'تعيين القوائم للأدوار', 'يسمح بربط القوائم والإجراءات بالأدوار', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Menus.Assign'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'عرض الإجراءات', 'يسمح بعرض إجراءات الأعمال', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Actions.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'إضافة إجراء', 'يسمح بإنشاء إجراءات أعمال جديدة', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Actions.Create'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'تعديل إجراء', 'يسمح بتعديل أسماء وأوصاف الإجراءات', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Actions.Edit'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'حذف إجراء', 'يسمح بحذف إجراءات الأعمال', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Actions.Delete'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'عرض الإعدادات', 'يسمح بعرض إعدادات النظام', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Settings.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'إدارة الإعدادات', 'يسمح بتحديث قيم إعدادات النظام', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.Settings.Manage'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";

INSERT INTO "IM_Translations" ("EntityType", "EntityId", "CultureCode", "Name", "Description", "CreatedBy")
SELECT 'Permission', p."Id", 'ar-SA', 'عرض سجلات التدقيق', 'يسمح بالاطلاع على سجلات التدقيق', 'SeedScript'
FROM "IM_Permissions" p WHERE p."Code" = 'Permissions.AuditLogs.View'
ON CONFLICT ("EntityType", "EntityId", "CultureCode") DO UPDATE SET "Name" = EXCLUDED."Name", "Description" = EXCLUDED."Description";
