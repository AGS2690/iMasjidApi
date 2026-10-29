using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImRoleMenuConfiguration : IEntityTypeConfiguration<ImRoleMenu>
{
    public void Configure(EntityTypeBuilder<ImRoleMenu> builder)
    {
        builder.ToTable("IM_RoleMenus");

        builder.HasKey(rm => rm.Id);

        builder.Property(rm => rm.CreatedBy)
            .HasMaxLength(100);

        builder.HasOne(rm => rm.Role)
            .WithMany(r => r.ImRoleMenus)
            .HasForeignKey(rm => rm.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rm => rm.Menu)
            .WithMany(m => m.ImRoleMenus)
            .HasForeignKey(rm => rm.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rm => rm.RoleId);
        builder.HasIndex(rm => new { rm.RoleId, rm.MenuId }).IsUnique();
    }
}
