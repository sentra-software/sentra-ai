namespace Sentra.SharedKernel.Abstractions;

/// <summary>
/// Represents a base entity with identity-based equality.
/// </summary>
/// <typeparam name="TId">The entity identifier type.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    public TId Id { get; }

    /// <summary>
    /// Determines whether the current entity is equal to another entity.
    /// </summary>
    /// <param name="other">The other entity.</param>
    /// <returns><see langword="true"/> if both entities are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType() &&
               EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Determines whether the current entity is equal to another object.
    /// </summary>
    /// <param name="obj">The other object.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for the entity.
    /// </summary>
    /// <returns>The computed hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    /// <summary>
    /// Determines whether two entities are equal.
    /// </summary>
    /// <param name="left">The left entity.</param>
    /// <param name="right">The right entity.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two entities are not equal.
    /// </summary>
    /// <param name="left">The left entity.</param>
    /// <param name="right">The right entity.</param>
    /// <returns><see langword="true"/> if not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}