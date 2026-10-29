using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImApplicationSettingConfiguration : IEntityTypeConfiguration<ImApplicationSetting>
{
    public void Configure(EntityTypeBuilder<ImApplicationSetting> builder)
    {
        builder.ToTable("IM_ApplicationSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Value)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(250);

        builder.Property(s => s.Module)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("General");

        builder.Property(s => s.CreatedBy)
            .HasMaxLength(100);

        builder.Property(s => s.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(s => s.DeletedBy)
            .HasMaxLength(100);

        builder.HasIndex(s => s.Key).IsUnique();
    }
}
