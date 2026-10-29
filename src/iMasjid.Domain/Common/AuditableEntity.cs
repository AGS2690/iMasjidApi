namespace iMasjid.Domain.Common;

/// <summary>
/// Base class for auditable entities that tracks creation, modification, and optional soft deletion.
/// </summary>
/// <typeparam name="TId">The type of the primary key.</typeparam>
public abstract class AuditableEntity<TId> : BaseEntity<TId>, ISoftDeletable
{
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
}
