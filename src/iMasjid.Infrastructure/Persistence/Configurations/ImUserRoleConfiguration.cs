using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iMasjid.Domain.Entities;

namespace iMasjid.Infrastructure.Persistence.Configurations;

public class ImUserRoleConfiguration : IEntityTypeConfiguration<ImUserRole>
{
    public void Configure(EntityTypeBuilder<ImUserRole> builder)
    {
        builder.ToTable("IM_UserRoles");

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.CreatedBy)
            .HasMaxLength(100);

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.ImUserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.ImUserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();
    }
}
