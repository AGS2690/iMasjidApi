using iMasjid.Domain.Common;

namespace iMasjid.Domain.Entities;

public class ImUserRole : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public ImUser User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public ImRole Role { get; set; } = null!;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}
