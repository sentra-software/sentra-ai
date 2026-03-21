using Microsoft.EntityFrameworkCore;
using Sentra.Application.Abstractions.Licensing;
using Sentra.Domain.Auditing;
using Sentra.Domain.DataSources;
using Sentra.Domain.Licensing;
using Sentra.Domain.Tenants;
using Sentra.Infrastructure.Persistence;
using Sentra.SharedKernel.Results;

namespace Sentra.Infrastructure.Licensing;

/// <summary>
/// Provides effective license state resolution for tenants.
/// </summary>
public sealed class CurrentTenantLicenseService : ICurrentTenantLicenseService
{
    private readonly SentraPlatformDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentTenantLicenseService"/> class.
    /// </summary>
    public CurrentTenantLicenseService(SentraPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<Result<TenantLicenseSnapshot>> GetSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<TenantLicenseSnapshot>(Error.Validation(
                "licensing.tenant.required",
                "Tenant identifier is required."));
        }

        TenantId typedTenantId = new(tenantId);

        TenantSubscription? subscription = await _dbContext.TenantSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == typedTenantId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<TenantLicenseSnapshot>(Error.Validation(
                "licensing.subscription.not_found",
                "No active tenant subscription was found."));
        }

        LicensePlan? plan = await _dbContext.LicensePlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == subscription.LicensePlanId, cancellationToken);

        if (plan is null || !plan.IsActive)
        {
            return Result.Failure<TenantLicenseSnapshot>(Error.Validation(
                "licensing.plan.not_found",
                "No active license plan was found for this tenant."));
        }

        DateTime utcNow = DateTime.UtcNow;
        DateTime monthStart = new(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        int usedQueriesThisMonth = await _dbContext.AiQueryAuditLogs
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == typedTenantId &&
                     x.CreatedAtUtc >= monthStart,
                cancellationToken);

        int managedDataSourceCount = await _dbContext.DataSources
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == typedTenantId &&
                     x.Status != DataSourceStatus.Disabled,
                cancellationToken);

        TenantLicenseSnapshot snapshot = new()
        {
            TenantId = tenantId,
            LicensePlanId = plan.Id.Value,
            PlanCode = plan.Code,
            PlanName = plan.Name,
            MaxManagedDataSources = plan.MaxManagedDataSources,
            MaxQueriesPerMonth = plan.MaxQueriesPerMonth,
            PreviewEnabled = plan.PreviewEnabled,
            MySqlEnabled = plan.MySqlEnabled,
            SqliteEnabled = plan.SqliteEnabled,
            UsedQueriesThisMonth = usedQueriesThisMonth,
            ManagedDataSourceCount = managedDataSourceCount,
            SubscriptionStatus = subscription.Status.ToString()
        };

        return Result.Success(snapshot);
    }

    /// <inheritdoc />
    public async Task<Result> EnsureQueryAllowedAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        Result<TenantLicenseSnapshot> snapshotResult =
            await GetSnapshotAsync(tenantId, cancellationToken);

        if (snapshotResult.IsFailure)
        {
            return Result.Failure(snapshotResult.Error);
        }

        TenantLicenseSnapshot snapshot = snapshotResult.ValueOrThrow();

        if (snapshot.UsedQueriesThisMonth >= snapshot.MaxQueriesPerMonth)
        {
            return Result.Failure(Error.Validation(
                "licensing.query_limit.exceeded",
                "The monthly query limit for this tenant has been reached."));
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> EnsureManagedDataSourceCreationAllowedAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        Result<TenantLicenseSnapshot> snapshotResult =
            await GetSnapshotAsync(tenantId, cancellationToken);

        if (snapshotResult.IsFailure)
        {
            return Result.Failure(snapshotResult.Error);
        }

        TenantLicenseSnapshot snapshot = snapshotResult.ValueOrThrow();

        if (snapshot.ManagedDataSourceCount >= snapshot.MaxManagedDataSources)
        {
            return Result.Failure(Error.Validation(
                "licensing.datasource_limit.exceeded",
                "The managed data source limit for this tenant has been reached."));
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> EnsurePreviewAllowedAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        Result<TenantLicenseSnapshot> snapshotResult =
            await GetSnapshotAsync(tenantId, cancellationToken);

        if (snapshotResult.IsFailure)
        {
            return Result.Failure(snapshotResult.Error);
        }

        TenantLicenseSnapshot snapshot = snapshotResult.ValueOrThrow();

        if (!snapshot.PreviewEnabled)
        {
            return Result.Failure(Error.Failure(
                "licensing.preview.disabled",
                "Preview mode is not enabled for this tenant."));
        }

        return Result.Success();
    }
}