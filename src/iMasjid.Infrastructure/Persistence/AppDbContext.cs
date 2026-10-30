using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using iMasjid.Application.Interfaces.Persistence;
using iMasjid.Application.Interfaces.Services;
using iMasjid.Domain.Common;
using iMasjid.Domain.Entities;
using iMasjid.Domain.Enums;

namespace iMasjid.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly IDateTimeProvider? _dateTimeProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
        : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public DbSet<ImUser> ImUsers => Set<ImUser>();
    public DbSet<ImRole> ImRoles => Set<ImRole>();
    public DbSet<ImPermission> ImPermissions => Set<ImPermission>();
    public DbSet<ImUserRole> ImUserRoles => Set<ImUserRole>();
    public DbSet<ImRolePermission> ImRolePermissions => Set<ImRolePermission>();
    public DbSet<ImRefreshToken> ImRefreshTokens => Set<ImRefreshToken>();
    public DbSet<ImUserSession> ImUserSessions => Set<ImUserSession>();
    public DbSet<ImLoginHistory> ImLoginHistories => Set<ImLoginHistory>();
    public DbSet<ImAuditLog> ImAuditLogs => Set<ImAuditLog>();
    public DbSet<ImApplicationSetting> ImApplicationSettings => Set<ImApplicationSetting>();
    public DbSet<ImMenu> ImMenus => Set<ImMenu>();
    public DbSet<ImBusinessAction> ImBusinessActions => Set<ImBusinessAction>();
    public DbSet<ImMenuAction> ImMenuActions => Set<ImMenuAction>();
    public DbSet<ImRoleMenu> ImRoleMenus => Set<ImRoleMenu>();
    public DbSet<ImRoleMenuAction> ImRoleMenuActions => Set<ImRoleMenuAction>();
    public DbSet<ImUserOtp> ImUserOtps => Set<ImUserOtp>();
    public DbSet<ImLocale> ImLocales => Set<ImLocale>();
    public DbSet<ImTranslation> ImTranslations => Set<ImTranslation>();
    public DbSet<VwUserEffectivePermission> VwUserEffectivePermissions => Set<VwUserEffectivePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<VwUserEffectivePermission>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_UserEffectivePermissions");
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");
            if (idProperty != null && idProperty.ClrType == typeof(Guid))
            {
                idProperty.SetDefaultValueSql("gen_random_uuid()");
                idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
            }

            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(property), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider?.UtcNow ?? DateTime.UtcNow;
        var currentUserName = _currentUserService?.UserName ?? "System";

        foreach (var entry in ChangeTracker.Entries<AuditableEntity<Guid>>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedOn = now;
                    entry.Entity.CreatedBy = currentUserName;
                    break;

                case EntityState.Modified:
                    if (entry.Entity.IsDeleted && entry.Property(p => p.IsDeleted).IsModified)
                    {
                        entry.Entity.DeletedOn = now;
                        entry.Entity.DeletedBy = currentUserName;
                    }
                    else
                    {
                        entry.Entity.ModifiedOn = now;
                        entry.Entity.ModifiedBy = currentUserName;
                    }
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
