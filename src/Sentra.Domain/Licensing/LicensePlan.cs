using Sentra.SharedKernel.Abstractions;
using Sentra.SharedKernel.Results;

namespace Sentra.Domain.Licensing;

/// <summary>
/// Represents a commercial license plan for the Sentra platform.
/// </summary>
public sealed class LicensePlan : AggregateRoot<LicensePlanId>
{
    private LicensePlan(
        LicensePlanId id,
        string name,
        string code,
        int maxManagedDataSources,
        int maxQueriesPerMonth,
        bool previewEnabled,
        int auditRetentionDays,
        bool mySqlEnabled,
        bool sqliteEnabled,
        decimal pricePerMonth,
        bool isActive)
        : base(id)
    {
        Name = name;
        Code = code;
        MaxManagedDataSources = maxManagedDataSources;
        MaxQueriesPerMonth = maxQueriesPerMonth;
        PreviewEnabled = previewEnabled;
        AuditRetentionDays = auditRetentionDays;
        MySqlEnabled = mySqlEnabled;
        SqliteEnabled = sqliteEnabled;
        PricePerMonth = pricePerMonth;
        IsActive = isActive;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the plan name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the plan code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the maximum number of managed data sources.
    /// </summary>
    public int MaxManagedDataSources { get; private set; }

    /// <summary>
    /// Gets the maximum number of monthly chat queries.
    /// </summary>
    public int MaxQueriesPerMonth { get; private set; }

    /// <summary>
    /// Gets a value indicating whether preview mode is enabled.
    /// </summary>
    public bool PreviewEnabled { get; private set; }

    /// <summary>
    /// Gets the number of audit retention days.
    /// </summary>
    public int AuditRetentionDays { get; private set; }

    /// <summary>
    /// Gets a value indicating whether MySQL support is enabled.
    /// </summary>
    public bool MySqlEnabled { get; private set; }

    /// <summary>
    /// Gets a value indicating whether SQLite support is enabled.
    /// </summary>
    public bool SqliteEnabled { get; private set; }

    /// <summary>
    /// Gets the monthly price.
    /// </summary>
    public decimal PricePerMonth { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the plan is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; }

    /// <summary>
    /// Creates a new license plan.
    /// </summary>
    public static Result Create(
        string name,
        string code,
        int maxManagedDataSources,
        int maxQueriesPerMonth,
        bool previewEnabled,
        int auditRetentionDays,
        bool mySqlEnabled,
        bool sqliteEnabled,
        decimal pricePerMonth)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation(
                "licensing.plan.name.required",
                "Plan name is required."));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure(Error.Validation(
                "licensing.plan.code.required",
                "Plan code is required."));
        }

        if (maxManagedDataSources < 1)
        {
            return Result.Failure(Error.Validation(
                "licensing.plan.datasources.invalid",
                "Max managed data sources must be at least 1."));
        }

        if (maxQueriesPerMonth < 1)
        {
            return Result.Failure(Error.Validation(
                "licensing.plan.queries.invalid",
                "Max queries per month must be at least 1."));
        }

        if (auditRetentionDays < 1)
        {
            return Result.Failure(Error.Validation(
                "licensing.plan.audit_retention.invalid",
                "Audit retention days must be at least 1."));
        }

        if (pricePerMonth < 0)
        {
            return Result.Failure(Error.Validation(
                "licensing.plan.price.invalid",
                "Price per month cannot be negative."));
        }

        LicensePlan plan = new(
            LicensePlanId.New(),
            name.Trim(),
            code.Trim().ToUpperInvariant(),
            maxManagedDataSources,
            maxQueriesPerMonth,
            previewEnabled,
            auditRetentionDays,
            mySqlEnabled,
            sqliteEnabled,
            pricePerMonth,
            true);

        return Result.Success(plan);
    }

    /// <summary>
    /// Deactivates the plan.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activates the plan.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
}