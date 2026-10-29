using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImMenuActionConfiguration : IEntityTypeConfiguration<ImMenuAction>
{
    public void Configure(EntityTypeBuilder<ImMenuAction> builder)
    {
        builder.ToTable("IM_MenuActions");

        builder.HasKey(ma => ma.Id);

        builder.Property(ma => ma.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ma => ma.CreatedBy)
            .HasMaxLength(100);

        builder.HasOne(ma => ma.Menu)
            .WithMany(m => m.ImMenuActions)
            .HasForeignKey(ma => ma.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ma => ma.Action)
            .WithMany(a => a.ImMenuActions)
            .HasForeignKey(ma => ma.ActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ma => ma.MenuId);
        builder.HasIndex(ma => new { ma.MenuId, ma.ActionId }).IsUnique();
    }
}
