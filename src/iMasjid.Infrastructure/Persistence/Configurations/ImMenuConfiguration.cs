using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImMenuConfiguration : IEntityTypeConfiguration<ImMenu>
{
    public void Configure(EntityTypeBuilder<ImMenu> builder)
    {
        builder.ToTable("IM_Menus");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Route)
            .HasMaxLength(256);

        builder.Property(m => m.Icon)
            .HasMaxLength(100);

        builder.Property(m => m.OrderIndex)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedBy)
            .HasMaxLength(100);

        builder.Property(m => m.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(m => m.DeletedBy)
            .HasMaxLength(100);

        builder.HasOne(m => m.ParentMenu)
            .WithMany(m => m.SubMenus)
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.ParentId);
    }
}
