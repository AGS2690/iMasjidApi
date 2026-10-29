using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImLocaleConfiguration : IEntityTypeConfiguration<ImLocale>
{
    public void Configure(EntityTypeBuilder<ImLocale> builder)
    {
        builder.ToTable("IM_Locales");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(l => l.CountryName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.PhoneCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.LanguageName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.CultureCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.CurrencySymbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.CurrencyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.CurrencyCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(l => l.Direction)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("LTR");

        builder.Property(l => l.FlagIconUrl)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedBy)
            .HasMaxLength(100);

        builder.Property(l => l.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(l => l.DeletedBy)
            .HasMaxLength(100);

        builder.HasIndex(l => l.CultureCode).IsUnique();
        builder.HasIndex(l => l.DisplayOrder);
    }
}
