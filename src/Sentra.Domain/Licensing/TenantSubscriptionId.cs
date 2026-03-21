namespace Sentra.Domain.Licensing;

/// <summary>
/// Represents the identifier of a tenant subscription.
/// </summary>
public readonly record struct TenantSubscriptionId(Guid Value)
{
    /// <summary>
    /// Creates a new tenant subscription identifier.
    /// </summary>
    public static TenantSubscriptionId New() => new TenantSubscriptionId(Guid.NewGuid());

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}