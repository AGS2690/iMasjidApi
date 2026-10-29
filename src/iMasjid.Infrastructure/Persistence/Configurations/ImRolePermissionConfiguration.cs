using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImRolePermissionConfiguration : IEntityTypeConfiguration<ImRolePermission>
{
    public void Configure(EntityTypeBuilder<ImRolePermission> builder)
    {
        builder.ToTable("IM_RolePermissions");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.CreatedBy)
            .HasMaxLength(100);

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.ImRolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.ImRolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique();
    }
}
