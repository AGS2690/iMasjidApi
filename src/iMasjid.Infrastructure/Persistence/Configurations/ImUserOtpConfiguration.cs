using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImUserOtpConfiguration : IEntityTypeConfiguration<ImUserOtp>
{
    public void Configure(EntityTypeBuilder<ImUserOtp> builder)
    {
        builder.ToTable("IM_UserOtps");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OtpCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(o => o.Purpose)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.CreatedByIp)
            .HasMaxLength(50);

        builder.Property(o => o.ExpiresOn)
            .IsRequired();

        builder.Property(o => o.IsUsed)
            .HasDefaultValue(false);

        builder.Property(o => o.AttemptsCount)
            .HasDefaultValue(0);

        builder.Property(o => o.MaxAttempts)
            .HasDefaultValue(5);

        builder.HasOne(o => o.User)
            .WithMany(u => u.ImUserOtps)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.UserId, o.Purpose, o.IsUsed });
        builder.HasIndex(o => o.ExpiresOn);

        builder.Ignore(o => o.IsExpired);
        builder.Ignore(o => o.IsValid);
    }
}
