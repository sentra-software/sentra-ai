namespace Sentra.Domain.Licensing;

/// <summary>
/// Represents the identifier of a license plan.
/// </summary>
public readonly record struct LicensePlanId(Guid Value)
{
    /// <summary>
    /// Creates a new license plan identifier.
    /// </summary>
    public static LicensePlanId New() => new LicensePlanId(Guid.NewGuid());

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}