namespace Sentra.Domain.Billing;

/// <summary>
/// Represents the identifier of a billing customer mapping.
/// </summary>
/// <param name="Value"></param>
public readonly record struct BillingCustomerId(Guid Value)
{
    /// <summary>
    /// Creates a new billing customer identifier.
    /// </summary>
    public static BillingCustomerId New() => new BillingCustomerId(Guid.NewGuid());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}