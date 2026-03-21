using Sentra.SharedKernel.Results;

namespace Sentra.Application.Abstractions.Licensing;

/// <summary>
/// Provides access to the effective license state of a tenant.
/// </summary>
public interface ICurrentTenantLicenseService
{
    /// <summary>
    /// Gets the current effective license snapshot for a tenant.
    /// </summary>
    Task<Result<TenantLicenseSnapshot>> GetSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the tenant can execute one more chat query.
    /// </summary>
    Task<Result> EnsureQueryAllowedAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the tenant can create one more managed data source.
    /// </summary>
    Task<Result> EnsureManagedDataSourceCreationAllowedAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that preview mode is enabled for the tenant.
    /// </summary>
    Task<Result> EnsurePreviewAllowedAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}