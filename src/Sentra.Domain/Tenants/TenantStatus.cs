namespace Sentra.Domain.Tenants;

/// <summary>
/// Represents the lifecycle status of a tenant.
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// The tenant is active and allowed to use the platform.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The tenant is inactive and not allowed to use the platform.
    /// </summary>
    Inactive = 2
}