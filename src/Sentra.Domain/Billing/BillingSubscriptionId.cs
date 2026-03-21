namespace Sentra.Domain.Billing;

/// <summary>
/// Represents the identifier of a billing subscription mapping.
/// </summary>
public readonly record struct BillingSubscriptionId(Guid Value)
{
    /// <summary>
    /// Creates a new billing subscription identifier.
    /// </summary>
    public static BillingSubscriptionId New() => new BillingSubscriptionId(Guid.NewGuid());

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}