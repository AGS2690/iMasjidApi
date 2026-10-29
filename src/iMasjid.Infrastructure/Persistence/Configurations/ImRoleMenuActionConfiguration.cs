using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImRoleMenuActionConfiguration : IEntityTypeConfiguration<ImRoleMenuAction>
{
    public void Configure(EntityTypeBuilder<ImRoleMenuAction> builder)
    {
        builder.ToTable("IM_RoleMenuActions");

        builder.HasKey(rma => rma.Id);

        builder.Property(rma => rma.CreatedBy)
            .HasMaxLength(100);

        builder.HasOne(rma => rma.RoleMenu)
            .WithMany(rm => rm.ImRoleMenuActions)
            .HasForeignKey(rma => rma.RoleMenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rma => rma.MenuAction)
            .WithMany(ma => ma.ImRoleMenuActions)
            .HasForeignKey(rma => rma.MenuActionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(rma => rma.RoleMenuId);
        builder.HasIndex(rma => new { rma.RoleMenuId, rma.MenuActionId }).IsUnique();
    }
}
