namespace Sentra.Application.Abstractions.Licensing;

/// <summary>
/// Represents the effective licensing state of a tenant.
/// </summary>
public sealed class TenantLicenseSnapshot
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the plan identifier.
    /// </summary>
    public Guid LicensePlanId { get; set; }

    /// <summary>
    /// Gets or sets the plan code.
    /// </summary>
    public string PlanCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plan name.
    /// </summary>
    public string PlanName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum allowed managed data sources.
    /// </summary>
    public int MaxManagedDataSources { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed monthly queries.
    /// </summary>
    public int MaxQueriesPerMonth { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether preview is enabled.
    /// </summary>
    public bool PreviewEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether MySQL is enabled.
    /// </summary>
    public bool MySqlEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether SQLite is enabled.
    /// </summary>
    public bool SqliteEnabled { get; set; }

    /// <summary>
    /// Gets or sets the number of used queries in the current UTC month.
    /// </summary>
    public int UsedQueriesThisMonth { get; set; }

    /// <summary>
    /// Gets or sets the number of managed data sources currently stored.
    /// </summary>
    public int ManagedDataSourceCount { get; set; }

    /// <summary>
    /// Gets or sets the subscription status.
    /// </summary>
    public string SubscriptionStatus { get; set; } = string.Empty;
}