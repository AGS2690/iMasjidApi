using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImLoginHistoryConfiguration : IEntityTypeConfiguration<ImLoginHistory>
{
    public void Configure(EntityTypeBuilder<ImLoginHistory> builder)
    {
        builder.ToTable("IM_LoginHistory");

        builder.HasKey(lh => lh.Id);

        builder.Property(lh => lh.UserNameOrEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(lh => lh.IpAddress)
            .HasMaxLength(50);

        builder.Property(lh => lh.UserAgent)
            .HasMaxLength(500);

        builder.Property(lh => lh.FailureReason)
            .HasMaxLength(250);

        builder.HasOne(lh => lh.User)
            .WithMany(u => u.ImLoginHistories)
            .HasForeignKey(lh => lh.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(lh => lh.UserId);
        builder.HasIndex(lh => lh.LoginTime);
    }
}
