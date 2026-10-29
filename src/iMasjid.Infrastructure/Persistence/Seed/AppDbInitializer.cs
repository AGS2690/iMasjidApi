using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Domain.Constants;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Seed;

public static class AppDbInitializer
{
    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
        try
        {
            // In DB-First approach, tables are created via SQL scripts in PostgreSQL / relational databases.
            // EnsureCreatedAsync is only invoked for InMemory / non-relational database providers.
            if (!context.Database.IsRelational())
            {
                await context.Database.EnsureCreatedAsync();
            }

            // 1. Seed Permissions
            var allPermissionCodes = Permissions.GetAllPermissions();
            var existingPermissions = await context.ImPermissions.IgnoreQueryFilters().ToListAsync();

            var newPermissions = new List<ImPermission>();
            foreach (var code in allPermissionCodes)
            {
                if (!existingPermissions.Any(p => p.Code == code))
                {
                    var parts = code.Split('.');
                    var module = parts.Length > 1 ? parts[1] : "General";
                    var name = parts.Length > 2 ? $"{parts[1]} {parts[2]}" : code;

                    var perm = new ImPermission
                    {
                        Code = code,
                        Name = name,
                        Module = module,
                        Description = $"Grants permission to {name}",
                        IsActive = true,
                        CreatedBy = "Seeder"
                    };
                    newPermissions.Add(perm);
                }
            }

            if (newPermissions.Count > 0)
            {
                await context.ImPermissions.AddRangeAsync(newPermissions);
                await context.SaveChangesAsync();
                existingPermissions.AddRange(newPermissions);
                logger.LogInformation("Seeded {Count} new permissions.", newPermissions.Count);
            }

            // 2. Seed Roles
            var existingRoles = await context.ImRoles.IgnoreQueryFilters().ToListAsync();
            var rolesToSeed = new[]
            {
                (Name: Roles.SuperAdmin, Description: "Super Administrator with unrestricted access"),
                (Name: Roles.MinistryAdmin, Description: "Ministry level administrator"),
                (Name: Roles.SuperMosqueAdmin, Description: "Super administrator across multiple mosques"),
                (Name: Roles.MosqueAdmin, Description: "Administrator for specific mosque operations"),
                (Name: Roles.MosqueAssistant, Description: "Assistant staff supporting mosque administration"),
                (Name: Roles.Imam, Description: "Mosque religious leader and prayer coordinator"),
                (Name: Roles.Member, Description: "Registered community mosque member"),
                (Name: Roles.OfficeBearer, Description: "Mosque committee or board office bearer"),
                (Name: Roles.SupportTeam, Description: "Technical and operational support team"),
                (Name: Roles.Developer, Description: "System and platform developer"),
                (Name: Roles.Vendor, Description: "External service provider or supplier"),
                (Name: Roles.Technician, Description: "Maintenance and hardware technician"),
                (Name: Roles.Accountant, Description: "Financial and accounting administrator")
            };

            foreach (var (name, description) in rolesToSeed)
            {
                if (!existingRoles.Any(r => r.NormalizedName == name.ToUpperInvariant()))
                {
                    var role = new ImRole
                    {
                        Name = name,
                        NormalizedName = name.ToUpperInvariant(),
                        Description = description,
                        IsActive = true,
                        CreatedBy = "Seeder"
                    };
                    context.ImRoles.Add(role);
                    existingRoles.Add(role);
                }
            }

            await context.SaveChangesAsync();

            // 3. Seed Role-Permissions
            var superAdminRole = existingRoles.FirstOrDefault(r => r.NormalizedName == "SUPER ADMIN" || r.NormalizedName == "SUPERADMIN") ?? existingRoles.First();
            var adminRole = existingRoles.FirstOrDefault(r => r.NormalizedName == "MOSQUE ADMIN" || r.NormalizedName == "ADMIN" || r.NormalizedName == "MINISTRY ADMIN");
            var userRole = existingRoles.FirstOrDefault(r => r.NormalizedName == "MEMBER" || r.NormalizedName == "USER");

            var existingRolePermissions = await context.ImRolePermissions.ToListAsync();

            // SuperAdmin gets all permissions
            foreach (var perm in existingPermissions)
            {
                if (!existingRolePermissions.Any(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == perm.Id))
                {
                    context.ImRolePermissions.Add(new ImRolePermission
                    {
                        RoleId = superAdminRole.Id,
                        PermissionId = perm.Id,
                        CreatedBy = "Seeder"
                    });
                }
            }

            // Admin gets user, role, and settings permissions
            if (adminRole != null)
            {
                var adminPermCodes = new[]
                {
                    Permissions.Users.View, Permissions.Users.Create, Permissions.Users.Edit, Permissions.Users.ManageRoles,
                    Permissions.Roles.View,
                    Permissions.Settings.View,
                    Permissions.AuditLogs.View
                };

                foreach (var perm in existingPermissions.Where(p => adminPermCodes.Contains(p.Code)))
                {
                    if (!existingRolePermissions.Any(rp => rp.RoleId == adminRole.Id && rp.PermissionId == perm.Id))
                    {
                        context.ImRolePermissions.Add(new ImRolePermission
                        {
                            RoleId = adminRole.Id,
                            PermissionId = perm.Id,
                            CreatedBy = "Seeder"
                        });
                    }
                }
            }

            // User gets basic view permissions
            if (userRole != null)
            {
                var userPermCodes = new[]
                {
                    Permissions.Users.View,
                    Permissions.Settings.View
                };

                foreach (var perm in existingPermissions.Where(p => userPermCodes.Contains(p.Code)))
                {
                    if (!existingRolePermissions.Any(rp => rp.RoleId == userRole.Id && rp.PermissionId == perm.Id))
                    {
                        context.ImRolePermissions.Add(new ImRolePermission
                        {
                            RoleId = userRole.Id,
                            PermissionId = perm.Id,
                            CreatedBy = "Seeder"
                        });
                    }
                }
            }

            await context.SaveChangesAsync();

            // 4. Seed Default SuperAdmin User
            var superAdminEmail = "superadmin@imasjid.com";
            var existingUser = await context.ImUsers.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserName == "superadmin" || u.Email == superAdminEmail);

            if (existingUser == null)
            {
                var adminUser = new ImUser
                {
                    FirstName = "System",
                    LastName = "SuperAdmin",
                    UserName = "superadmin",
                    Email = superAdminEmail,
                    PhoneNumber = "+1234567890",
                    PasswordHash = passwordHasher.HashPassword("SuperAdmin123!"),
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedBy = "Seeder"
                };

                context.ImUsers.Add(adminUser);
                await context.SaveChangesAsync();

                // Assign SuperAdmin Role
                context.ImUserRoles.Add(new ImUserRole
                {
                    UserId = adminUser.Id,
                    RoleId = superAdminRole.Id,
                    CreatedBy = "Seeder"
                });

                await context.SaveChangesAsync();
                logger.LogInformation("Default SuperAdmin user created: {Email}", superAdminEmail);
            }
            else if (existingUser.Email != superAdminEmail)
            {
                existingUser.Email = superAdminEmail;
                await context.SaveChangesAsync();
                logger.LogInformation("Updated SuperAdmin email to: {Email}", superAdminEmail);
            }

            // 5. Seed Default Application Settings
            var defaultSettings = new[]
            {
                (Key: "Security.PasswordExpiryDays", Value: "90", Description: "Days after which user password expires", Module: "Security"),
                (Key: "System.AppName", Value: "iMasjid Enterprise API", Description: "Display name of the application", Module: "General"),
                (Key: "System.EnableAuditLogs", Value: "true", Description: "Flag indicating whether audit logging is enabled", Module: "Auditing")
            };

            var existingSettings = await context.ImApplicationSettings.IgnoreQueryFilters().ToListAsync();
            foreach (var (key, value, description, module) in defaultSettings)
            {
                if (!existingSettings.Any(s => s.Key.Equals(key, StringComparison.OrdinalIgnoreCase)))
                {
                    context.ImApplicationSettings.Add(new ImApplicationSetting
                    {
                        Key = key,
                        Value = value,
                        Description = description,
                        Module = module,
                        CreatedBy = "Seeder"
                    });
                }
            }

            await context.SaveChangesAsync();

            // 6. Seed Supported Locales
            if (context.Database.IsRelational())
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""IM_Locales"" (
                        ""Id"" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                        ""DisplayOrder"" INT NOT NULL DEFAULT 1,
                        ""CountryName"" VARCHAR(100) NOT NULL,
                        ""PhoneCode"" VARCHAR(20) NOT NULL,
                        ""LanguageName"" VARCHAR(100) NOT NULL,
                        ""CultureCode"" VARCHAR(20) NOT NULL,
                        ""CurrencySymbol"" VARCHAR(20) NOT NULL,
                        ""CurrencyName"" VARCHAR(100) NOT NULL,
                        ""CurrencyCode"" VARCHAR(10) NOT NULL,
                        ""Direction"" VARCHAR(10) NOT NULL DEFAULT 'LTR',
                        ""FlagIconUrl"" VARCHAR(255) NOT NULL,
                        ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""CreatedOn"" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""CreatedBy"" VARCHAR(100) NULL,
                        ""ModifiedOn"" TIMESTAMPTZ NULL,
                        ""ModifiedBy"" VARCHAR(100) NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""DeletedOn"" TIMESTAMPTZ NULL,
                        ""DeletedBy"" VARCHAR(100) NULL
                    );
                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_IM_Locales_CultureCode"" ON ""IM_Locales"" (""CultureCode"");
                    CREATE INDEX IF NOT EXISTS ""IX_IM_Locales_DisplayOrder"" ON ""IM_Locales"" (""DisplayOrder"");
                ");
            }

            var defaultLocales = new[]
            {
                new ImLocale
                {
                    Id = Guid.Parse("5147bd8b-79a2-4e72-95ca-4a33fae51d6d"),
                    DisplayOrder = 1,
                    CountryName = "United States",
                    PhoneCode = "+1",
                    LanguageName = "English (US)",
                    CultureCode = "en-US",
                    CurrencySymbol = "$",
                    CurrencyName = "US Dollar",
                    CurrencyCode = "USD",
                    Direction = "LTR",
                    FlagIconUrl = "assets/flags/EN.svg",
                    IsActive = true,
                    CreatedBy = "Seeder"
                },
                new ImLocale
                {
                    Id = Guid.Parse("7185939a-a861-4435-b04a-75a004831027"),
                    DisplayOrder = 2,
                    CountryName = "Saudi Arabia",
                    PhoneCode = "+966",
                    LanguageName = "عربي",
                    CultureCode = "ar-SA",
                    CurrencySymbol = "﷼",
                    CurrencyName = "Saudi Riyal",
                    CurrencyCode = "SAR",
                    Direction = "RTL",
                    FlagIconUrl = "assets/flags/AR.svg",
                    IsActive = true,
                    CreatedBy = "Seeder"
                },
                new ImLocale
                {
                    Id = Guid.Parse("9d0e2b08-2365-42bb-b2b8-aaffa57ec2fe"),
                    DisplayOrder = 3,
                    CountryName = "Indonesia",
                    PhoneCode = "+62",
                    LanguageName = "Bahasa",
                    CultureCode = "id-ID",
                    CurrencySymbol = "Rp",
                    CurrencyName = "Indonesian Rupiah",
                    CurrencyCode = "IDR",
                    Direction = "LTR",
                    FlagIconUrl = "assets/flags/BI.png",
                    IsActive = true,
                    CreatedBy = "Seeder"
                },
                new ImLocale
                {
                    Id = Guid.Parse("0cb80eed-e33f-4067-a2b4-21dd38782367"),
                    DisplayOrder = 4,
                    CountryName = "India",
                    PhoneCode = "+91",
                    LanguageName = "English (IN)",
                    CultureCode = "en-IN",
                    CurrencySymbol = "₹",
                    CurrencyName = "Indian Rupee",
                    CurrencyCode = "INR",
                    Direction = "LTR",
                    FlagIconUrl = "assets/flags/IN.svg",
                    IsActive = false,
                    CreatedBy = "Seeder"
                }
            };

            var existingLocales = await context.ImLocales.IgnoreQueryFilters().ToListAsync();
            foreach (var locale in defaultLocales)
            {
                if (!existingLocales.Any(l => l.Id == locale.Id || l.CultureCode.Equals(locale.CultureCode, StringComparison.OrdinalIgnoreCase)))
                {
                    context.ImLocales.Add(locale);
                }
            }

            await context.SaveChangesAsync();

            // 7. Seed Dynamic Translations (Menus, BusinessActions, Permissions)
            if (context.Database.IsRelational())
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""IM_Translations"" (
                        ""Id"" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                        ""EntityType"" VARCHAR(50) NOT NULL,
                        ""EntityId"" UUID NOT NULL,
                        ""CultureCode"" VARCHAR(20) NOT NULL,
                        ""Name"" VARCHAR(250) NOT NULL,
                        ""Description"" TEXT NULL,
                        ""CreatedOn"" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""CreatedBy"" VARCHAR(100) NULL,
                        ""ModifiedOn"" TIMESTAMPTZ NULL,
                        ""ModifiedBy"" VARCHAR(100) NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""DeletedOn"" TIMESTAMPTZ NULL,
                        ""DeletedBy"" VARCHAR(100) NULL,
                        CONSTRAINT ""UQ_IM_Translations_Entity_Culture"" UNIQUE (""EntityType"", ""EntityId"", ""CultureCode"")
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_IM_Translations_Lookup"" ON ""IM_Translations"" (""EntityType"", ""CultureCode"");
                    CREATE INDEX IF NOT EXISTS ""IX_IM_Translations_EntityId"" ON ""IM_Translations"" (""EntityId"");
                ");
            }

            await SeedTranslationsAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not run database seeder on startup. In DB-First approach, ensure 'InitialTableSchema.sql' and 'InitialDataInsert.sql' have been executed in your SQL Server database.");
        }
    }

    private static async Task SeedTranslationsAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            var existingTranslations = await context.ImTranslations.IgnoreQueryFilters().ToListAsync();

            // 7.1 Menus Translations Map
            var allMenus = await context.ImMenus.IgnoreQueryFilters().ToListAsync();
            var menuTranslationDefs = new Dictionary<string, (string IdName, string IdDesc, string ArName, string ArDesc)>
            {
                { "/dashboard", ("Dasbor", "Ringkasan operasional dan statistik", "لوحة القيادة", "ملخص العمليات والإحصائيات العامة") },
                { "Administration", ("Administrasi", "Menu administrasi dan manajemen peran", "الإدارة", "إدارة النظام والأدوار والصلاحيات") },
                { "Settings & System", ("Sistem & Pengaturan", "Konfigurasi sistem dan log audit", "النظام والإعدادات", "إعدادات النظام وسجلات التدقيق") },
                { "/admin/users", ("Manajemen Pengguna", "Kelola akun pengguna, peran, dan status", "إدارة المستخدمين", "إدارة حسابات المستخدمين وحالاتهم") },
                { "/admin/roles", ("Peran & Izin Perizinan", "Atur hak akses peran dan matriks izin", "الأدوار والصلاحيات", "تحديد أدوار المستخدمين ومصفوفة الأذونات") },
                { "/admin/menus", ("Pengaturan Menu", "Kelola hierarki menu navigasi dan perizinan", "إعداد القوائم", "تنظيم هيكل القوائم وربط العمليات") },
                { "/admin/actions", ("Aksi Bisnis", "Master aksi operasional CRUD dan ekspor", "إجراءات الأعمال", "إدارة العمليات الوظيفية والتصدير") },
                { "/system/settings", ("Pengaturan Aplikasi", "Konfigurasi parameter dan kebijakan sistem", "إعدادات التطبيق", "تكوين المعلمات وسياسات الأمان") },
                { "/system/audit-logs", ("Log Audit", "Riwayat aktivitas dan catatan perubahan data", "سجلات التدقيق", "تاريخ العمليات والتغييرات على النظام") },
            };

            foreach (var menu in allMenus)
            {
                string key = menu.Route ?? menu.Title;
                if (!menuTranslationDefs.TryGetValue(key, out var def))
                    continue;

                // id-ID
                if (!existingTranslations.Any(t => t.EntityType == "Menu" && t.EntityId == menu.Id && t.CultureCode == "id-ID"))
                {
                    context.ImTranslations.Add(new ImTranslation
                    {
                        EntityType = "Menu",
                        EntityId = menu.Id,
                        CultureCode = "id-ID",
                        Name = def.IdName,
                        Description = def.IdDesc,
                        CreatedBy = "Seeder"
                    });
                }
                // ar-SA
                if (!existingTranslations.Any(t => t.EntityType == "Menu" && t.EntityId == menu.Id && t.CultureCode == "ar-SA"))
                {
                    context.ImTranslations.Add(new ImTranslation
                    {
                        EntityType = "Menu",
                        EntityId = menu.Id,
                        CultureCode = "ar-SA",
                        Name = def.ArName,
                        Description = def.ArDesc,
                        CreatedBy = "Seeder"
                    });
                }
            }

            // 7.2 Business Actions Translations Map
            var allActions = await context.ImBusinessActions.IgnoreQueryFilters().ToListAsync();
            var actionTranslationDefs = new Dictionary<string, (string IdName, string IdDesc, string ArName, string ArDesc)>
            {
                { "VIEW", ("Lihat / Baca", "Mengizinkan melihat data dan mengakses layar antarmuka", "عرض / قراءة", "يسمح بعرض السجلات والوصول إلى الشاشات") },
                { "CREATE", ("Tambah Baru", "Mengizinkan membuat data dan entri baru", "إضافة جديد", "يسمح بإنشاء سجلات وبيانات جديدة") },
                { "EDIT", ("Ubah / Edit", "Mengizinkan mengubah dan memperbarui data yang ada", "تعديل / تحديث", "يسمح بتعديل وتحديث البيانات الموجودة") },
                { "DELETE", ("Hapus Data", "Mengizinkan menghapus data dari sistem", "حذف السجل", "يسمح بحذف السجلات من النظام") },
                { "EXPORT", ("Ekspor Data", "Mengizinkan mengunduh laporan, Excel, dan dokumen PDF", "تصدير البيانات", "يسمح بتصدير التقارير وجداول Excel وملفات PDF") },
                { "IMPORT", ("Impor Data", "Mengizinkan unggah dan impor data massal", "استيراد البيانات", "يسمح برفع واستيراد البيانات بشكل مجمع") }
            };

            foreach (var action in allActions)
            {
                if (!actionTranslationDefs.TryGetValue(action.Code.ToUpperInvariant(), out var def))
                    continue;

                // id-ID
                if (!existingTranslations.Any(t => t.EntityType == "BusinessAction" && t.EntityId == action.Id && t.CultureCode == "id-ID"))
                {
                    context.ImTranslations.Add(new ImTranslation
                    {
                        EntityType = "BusinessAction",
                        EntityId = action.Id,
                        CultureCode = "id-ID",
                        Name = def.IdName,
                        Description = def.IdDesc,
                        CreatedBy = "Seeder"
                    });
                }
                // ar-SA
                if (!existingTranslations.Any(t => t.EntityType == "BusinessAction" && t.EntityId == action.Id && t.CultureCode == "ar-SA"))
                {
                    context.ImTranslations.Add(new ImTranslation
                    {
                        EntityType = "BusinessAction",
                        EntityId = action.Id,
                        CultureCode = "ar-SA",
                        Name = def.ArName,
                        Description = def.ArDesc,
                        CreatedBy = "Seeder"
                    });
                }
            }

            // 7.3 Permissions Translations Map
            var allPermissions = await context.ImPermissions.IgnoreQueryFilters().ToListAsync();
            var permTranslationDefs = new Dictionary<string, (string IdName, string IdDesc, string ArName, string ArDesc)>
            {
                { "Permissions.Users.View", ("Lihat Pengguna", "Mengizinkan melihat daftar pengguna", "عرض المستخدمين", "يسمح بعرض قائمة المستخدمين") },
                { "Permissions.Users.Create", ("Tambah Pengguna", "Mengizinkan membuat akun pengguna baru", "إضافة مستخدم", "يسمح بإنشاء حسابات مستخدمين جديدة") },
                { "Permissions.Users.Edit", ("Ubah Pengguna", "Mengizinkan memperbarui data akun pengguna", "تعديل مستخدم", "يسمح بتعديل بيانات حساب المستخدم") },
                { "Permissions.Users.Delete", ("Hapus Pengguna", "Mengizinkan menghapus akun pengguna", "حذف مستخدم", "يسمح بحذف حساب المستخدم من النظام") },
                { "Permissions.Users.ManageRoles", ("Kelola Peran Pengguna", "Mengizinkan menetapkan peran kepada pengguna", "إدارة أدوار المستخدم", "يسمح بتعيين الأدوار للمستخدمين") },
                { "Permissions.Roles.View", ("Lihat Peran", "Mengizinkan melihat daftar peran sistem", "عرض الأدوار", "يسمح بعرض قائمة الأدوار في النظام") },
                { "Permissions.Roles.Create", ("Tambah Peran", "Mengizinkan membuat peran baru", "إضافة دور", "يسمح بإنشاء أدوار جديدة في النظام") },
                { "Permissions.Roles.Edit", ("Ubah Peran", "Mengizinkan memperbarui nama dan keterangan peran", "تعديل دور", "يسمح بتحديث بيانات ووصف الأدوار") },
                { "Permissions.Roles.Delete", ("Hapus Peran", "Mengizinkan menghapus peran dari sistem", "حذف دور", "يسمح بحذف الأدوار من النظام") },
                { "Permissions.Roles.ManagePermissions", ("Kelola Izin Peran", "Mengizinkan menetapkan izin ke peran pengguna", "إدارة صلاحيات الدور", "يسمح بتعيين الصلاحيات لأدوار النظام") },
                { "Permissions.Menus.View", ("Lihat Menu", "Mengizinkan melihat daftar menu navigasi", "عرض القوائم", "يسمح بعرض قوائم التنقل") },
                { "Permissions.Menus.Create", ("Tambah Menu", "Mengizinkan menambah menu navigasi baru", "إضافة قائمة", "يسمح بإنشاء قوائم تنقل جديدة") },
                { "Permissions.Menus.Edit", ("Ubah Menu", "Mengizinkan mengubah rute dan urutan menu", "تعديل قائمة", "يسمح بتعديل مسارات وعناوين القوائم") },
                { "Permissions.Menus.Delete", ("Hapus Menu", "Mengizinkan menghapus menu navigasi", "حذف قائمة", "يسمح بحذف القوائم") },
                { "Permissions.Menus.Assign", ("Tetapkan Menu Peran", "Mengizinkan menetapkan menu dan aksi ke peran", "تعيين القوائم للأدوار", "يسمح بربط القوائم والإجراءات بالأدوار") },
                { "Permissions.Actions.View", ("Lihat Aksi", "Mengizinkan melihat daftar aksi bisnis", "عرض الإجراءات", "يسمح بعرض إجراءات الأعمال") },
                { "Permissions.Actions.Create", ("Tambah Aksi", "Mengizinkan membuat aksi bisnis baru", "إضافة إجراء", "يسمح بإنشاء إجراءات أعمال جديدة") },
                { "Permissions.Actions.Edit", ("Ubah Aksi", "Mengizinkan mengubah kode dan nama aksi bisnis", "تعديل إجراء", "يسمح بتعديل أسماء وأوصاف الإجراءات") },
                { "Permissions.Actions.Delete", ("Hapus Aksi", "Mengizinkan menghapus aksi bisnis", "حذف إجراء", "يسمح بحذف إجراءات الأعمال") },
                { "Permissions.Settings.View", ("Lihat Pengaturan", "Mengizinkan melihat pengaturan sistem", "عرض الإعدادات", "يسمح بعرض إعدادات النظام") },
                { "Permissions.Settings.Manage", ("Kelola Pengaturan", "Mengizinkan memperbarui nilai pengaturan aplikasi", "إدارة الإعدادات", "يسمح بتحديث قيم إعدادات النظام") },
                { "Permissions.AuditLogs.View", ("Lihat Log Audit", "Mengizinkan memeriksa catatan audit aktivitas", "عرض سجلات التدقيق", "يسمح بالاطلاع على سجلات التدقيق") }
            };

            foreach (var perm in allPermissions)
            {
                if (!permTranslationDefs.TryGetValue(perm.Code, out var def))
                    continue;

                // id-ID
                if (!existingTranslations.Any(t => t.EntityType == "Permission" && t.EntityId == perm.Id && t.CultureCode == "id-ID"))
                {
                    context.ImTranslations.Add(new ImTranslation
                    {
                        EntityType = "Permission",
                        EntityId = perm.Id,
                        CultureCode = "id-ID",
                        Name = def.IdName,
                        Description = def.IdDesc,
                        CreatedBy = "Seeder"
                    });
                }
                // ar-SA
                if (!existingTranslations.Any(t => t.EntityType == "Permission" && t.EntityId == perm.Id && t.CultureCode == "ar-SA"))
                {
                    context.ImTranslations.Add(new ImTranslation
                    {
                        EntityType = "Permission",
                        EntityId = perm.Id,
                        CultureCode = "ar-SA",
                        Name = def.ArName,
                        Description = def.ArDesc,
                        CreatedBy = "Seeder"
                    });
                }
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not seed dynamic translations.");
        }
    }
}
