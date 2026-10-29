using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImBusinessActionConfiguration : IEntityTypeConfiguration<ImBusinessAction>
{
    public void Configure(EntityTypeBuilder<ImBusinessAction> builder)
    {
        builder.ToTable("IM_BusinessActions");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Description)
            .HasMaxLength(250);

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(100);

        builder.Property(a => a.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(a => a.DeletedBy)
            .HasMaxLength(100);

        builder.HasIndex(a => a.Code).IsUnique();
    }
}
