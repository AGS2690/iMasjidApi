using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImRoleConfiguration : IEntityTypeConfiguration<ImRole>
{
    public void Configure(EntityTypeBuilder<ImRole> builder)
    {
        builder.ToTable("IM_Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.NormalizedName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .HasMaxLength(250);

        builder.Property(r => r.CreatedBy)
            .HasMaxLength(100);

        builder.Property(r => r.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(r => r.DeletedBy)
            .HasMaxLength(100);

        builder.HasIndex(r => r.NormalizedName).IsUnique();
    }
}
