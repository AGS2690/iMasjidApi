namespace iMasjid.Domain.Common;

/// <summary>
/// Interface for entities supporting soft deletion.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedOn { get; set; }
    string? DeletedBy { get; set; }
}
