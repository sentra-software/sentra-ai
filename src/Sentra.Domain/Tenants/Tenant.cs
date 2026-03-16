using Sentra.SharedKernel.Abstractions;
using Sentra.SharedKernel.Results;

namespace Sentra.Domain.Tenants;

/// <summary>
/// Represents an organization that uses the Sentra platform.
/// </summary>
public sealed class Tenant : AggregateRoot<TenantId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tenant"/> class.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="name">The tenant name.</param>
    /// <param name="status">The current tenant status.</param>
    private Tenant(TenantId id, string name, TenantStatus status)
        : base(id)
    {
        Name = name;
        Status = status;
    }

    /// <summary>
    /// Gets the tenant name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the current tenant status.
    /// </summary>
    public TenantStatus Status { get; private set; }

    /// <summary>
    /// Creates a new tenant instance.
    /// </summary>
    /// <param name="name">The tenant name.</param>
    /// <returns>
    /// A successful result containing the created <see cref="Tenant"/>,
    /// or a failed result when validation fails.
    /// </returns>
    public static Result<Tenant> Create(string name)
    {
        Result? validationResult = ValidateName(name);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Tenant>(validationResult.Error);
        }

        Tenant? tenant = new Tenant(TenantId.New(), name.Trim(), TenantStatus.Active);

        return Result.Success(tenant);
    }

    /// <summary>
    /// Changes the tenant name.
    /// </summary>
    /// <param name="name">The new tenant name.</param>
    /// <returns>A result indicating whether the operation succeeded.</returns>
    public Result Rename(string name)
    {
        Result? validationResult = ValidateName(name);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        Name = name.Trim();

        return Result.Success();
    }

    /// <summary>
    /// Marks the tenant as active.
    /// </summary>
    public void Activate()
    {
        Status = TenantStatus.Active;
    }

    /// <summary>
    /// Marks the tenant as inactive.
    /// </summary>
    public void Deactivate()
    {
        Status = TenantStatus.Inactive;
    }

    /// <summary>
    /// Validates a tenant name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <returns>A result indicating whether the name is valid.</returns>
    private static Result ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(
                Error.Validation(
                    "tenants.name.required",
                    "Tenant name is required."));
        }

        string? trimmedName = name.Trim();

        if (trimmedName.Length < 2)
        {
            return Result.Failure(
                Error.Validation(
                    "tenants.name.too_short",
                    "Tenant name must be at least 2 characters long."));
        }

        if (trimmedName.Length > 200)
        {
            return Result.Failure(
                Error.Validation(
                    "tenants.name.too_long",
                    "Tenant name must not exceed 200 characters."));
        }

        return Result.Success();
    }
}