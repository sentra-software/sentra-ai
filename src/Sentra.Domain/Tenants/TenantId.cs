namespace Sentra.Domain.Tenants;

/// <summary>
/// Represents the unique identifier of a tenant.
/// </summary>
public readonly record struct TenantId(Guid Value)
{
    /// <summary>
    /// Creates a new tenant identifier.
    /// </summary>
    /// <returns>A new <see cref="TenantId"/> instance.</returns>
    public static TenantId New() => new(Guid.NewGuid());

    /// <summary>
    /// Returns the string representation of the tenant identifier.
    /// </summary>
    /// <returns>The identifier as a string.</returns>
    public override string ToString() => Value.ToString();
}