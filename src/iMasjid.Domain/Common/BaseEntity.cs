namespace iMasjid.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// </summary>
/// <typeparam name="TId">The type of the primary key.</typeparam>
public abstract class BaseEntity<TId>
{
    public TId Id { get; set; } = default!;

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (EqualityComparer<TId>.Default.Equals(Id, default) || EqualityComparer<TId>.Default.Equals(other.Id, default))
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }
}
