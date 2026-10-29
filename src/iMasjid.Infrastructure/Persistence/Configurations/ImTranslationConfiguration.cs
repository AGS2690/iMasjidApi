using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImTranslationConfiguration : IEntityTypeConfiguration<ImTranslation>
{
    public void Configure(EntityTypeBuilder<ImTranslation> builder)
    {
        builder.ToTable("IM_Translations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.EntityId)
            .IsRequired();

        builder.Property(t => t.CultureCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(t => t.Description)
            .HasColumnType("text");

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(100);

        builder.Property(t => t.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(t => t.DeletedBy)
            .HasMaxLength(100);

        builder.HasIndex(t => new { t.EntityType, t.EntityId, t.CultureCode })
            .IsUnique();

        builder.HasIndex(t => new { t.EntityType, t.CultureCode });
        builder.HasIndex(t => t.EntityId);
    }
}
