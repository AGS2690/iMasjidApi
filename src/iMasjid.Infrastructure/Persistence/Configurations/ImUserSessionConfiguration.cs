using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImUserSessionConfiguration : IEntityTypeConfiguration<ImUserSession>
{
    public void Configure(EntityTypeBuilder<ImUserSession> builder)
    {
        builder.ToTable("IM_UserSessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SessionToken)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.DeviceId)
            .HasMaxLength(100);

        builder.Property(s => s.DeviceName)
            .HasMaxLength(150);

        builder.Property(s => s.IpAddress)
            .HasMaxLength(50);

        builder.Property(s => s.UserAgent)
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(s => s.User)
            .WithMany(u => u.ImUserSessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.SessionToken).IsUnique();
    }
}
