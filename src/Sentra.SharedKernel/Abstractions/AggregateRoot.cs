namespace Sentra.SharedKernel.Abstractions;

/// <summary>
/// Represents a base aggregate root.
/// </summary>
/// <typeparam name="TId">The aggregate identifier type.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    protected AggregateRoot(TId id)
        : base(id)
    {
    }
}