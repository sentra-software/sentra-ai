using Sentra.Domain.Tenants;
using Sentra.SharedKernel.Abstractions;
using Sentra.SharedKernel.Results;

namespace Sentra.Domain.DataSources;

/// <summary>
/// Represents a database connection registered by a tenant.
/// </summary>
public sealed class DataSource : AggregateRoot<DataSourceId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataSource"/> class.
    /// </summary>
    private DataSource(
        DataSourceId id,
        TenantId tenantId,
        string name,
        DataSourceType type,
        string encryptedConnectionString)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Type = type;
        EncryptedConnectionString = encryptedConnectionString;
        Status = DataSourceStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// Gets the data source name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the data source type.
    /// </summary>
    public DataSourceType Type { get; }

    /// <summary>
    /// Gets the encrypted connection string.
    /// </summary>
    public string EncryptedConnectionString { get; private set; }

    /// <summary>
    /// Gets the current connection status.
    /// </summary>
    public DataSourceStatus Status { get; private set; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Creates a new data source instance.
    /// </summary>
    public static Result<DataSource> Create(
        TenantId tenantId,
        string name,
        DataSourceType type,
        string encryptedConnectionString)
    {
        Result? tenantValidation = ValidateTenantId(tenantId);
        if (tenantValidation.IsFailure)
            return Result.Failure<DataSource>(tenantValidation.Error);

        Result? nameValidation = ValidateName(name);
        if (nameValidation.IsFailure)
            return Result.Failure<DataSource>(nameValidation.Error);

        Result? connectionValidation = ValidateConnectionString(encryptedConnectionString);
        if (connectionValidation.IsFailure)
            return Result.Failure<DataSource>(connectionValidation.Error);

        DataSource? dataSource = new DataSource(
            DataSourceId.New(),
            tenantId,
            name.Trim(),
            type,
            encryptedConnectionString);

        return Result.Success(dataSource);
    }

    /// <summary>
    /// Renames the data source.
    /// </summary>
    public Result Rename(string name)
    {
        Result? validation = ValidateName(name);
        if (validation.IsFailure)
            return validation;

        Name = name.Trim();

        return Result.Success();
    }

    /// <summary>
    /// Marks the data source as active.
    /// </summary>
    public void Activate()
    {
        Status = DataSourceStatus.Active;
    }

    /// <summary>
    /// Marks the data source as disabled.
    /// </summary>
    public void Disable()
    {
        Status = DataSourceStatus.Disabled;
    }

    /// <summary>
    /// Updates the encrypted connection string.
    /// </summary>
    public Result UpdateConnectionString(string encryptedConnectionString)
    {
        Result? validation = ValidateConnectionString(encryptedConnectionString);
        if (validation.IsFailure)
            return validation;

        EncryptedConnectionString = encryptedConnectionString;

        return Result.Success();
    }

    private static Result ValidateTenantId(TenantId tenantId)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Result.Failure(
                Error.Validation(
                    "datasource.tenant.required",
                    "Tenant identifier is required."));
        }

        return Result.Success();
    }

    private static Result ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(
                Error.Validation(
                    "datasource.name.required",
                    "Data source name is required."));
        }

        string? trimmed = name.Trim();

        if (trimmed.Length < 2)
        {
            return Result.Failure(
                Error.Validation(
                    "datasource.name.short",
                    "Data source name must be at least 2 characters."));
        }

        if (trimmed.Length > 200)
        {
            return Result.Failure(
                Error.Validation(
                    "datasource.name.long",
                    "Data source name must not exceed 200 characters."));
        }

        return Result.Success();
    }

    private static Result ValidateConnectionString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure(
                Error.Validation(
                    "datasource.connection.required",
                    "Encrypted connection string is required."));
        }

        if (value.Length < 10)
        {
            return Result.Failure(
                Error.Validation(
                    "datasource.connection.invalid",
                    "Encrypted connection string appears invalid."));
        }

        return Result.Success();
    }
}