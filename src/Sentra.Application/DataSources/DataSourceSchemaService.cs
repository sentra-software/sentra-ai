using Sentra.Application.Abstractions.Connectors;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.DataSources;

/// <summary>
/// Represents the default application service for reading data source schemas.
/// </summary>
public sealed class DataSourceSchemaService : IDataSourceSchemaService
{
    private readonly IConnectorRegistry _connectorRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSourceSchemaService"/> class.
    /// </summary>
    /// <param name="connectorRegistry">The connector registry.</param>
    public DataSourceSchemaService(IConnectorRegistry connectorRegistry)
    {
        ArgumentNullException.ThrowIfNull(connectorRegistry);
        _connectorRegistry = connectorRegistry;
    }

    /// <summary>
    /// Reads the schema for the specified data source type using the matching connector.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The raw connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the discovered tables when the connector is resolved
    /// and schema discovery succeeds; otherwise, a failed result describing the error.
    /// </returns>
    public async Task<Result<IReadOnlyCollection<TableSchema>>> ReadSchemaAsync(
        DataSourceType dataSourceType,
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        Result connectionStringValidationResult = ValidateConnectionString(connectionString);

        if (connectionStringValidationResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<TableSchema>>(connectionStringValidationResult.Error);
        }

        Result<ConnectorType> connectorTypeResult = MapToConnectorType(dataSourceType);

        if (connectorTypeResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<TableSchema>>(connectorTypeResult.Error);
        }

        Result<IDataConnector> connectorResult = _connectorRegistry.GetConnector(connectorTypeResult.ValueOrThrow());

        if (connectorResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<TableSchema>>(connectorResult.Error);
        }

        IDataConnector connector = connectorResult.ValueOrThrow();

        IReadOnlyCollection<TableSchema> schema = await connector.ReadSchemaAsync(
            connectionString.Trim(),
            cancellationToken);

        return Result.Success(schema);
    }

    /// <summary>
    /// Maps a domain data source type to a connector type.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <returns>
    /// A successful result containing the mapped <see cref="ConnectorType"/>,
    /// or a failed result when the type is invalid or unsupported.
    /// </returns>
    private static Result<ConnectorType> MapToConnectorType(DataSourceType dataSourceType)
    {
        if (!Enum.IsDefined(dataSourceType))
        {
            return Result.Failure<ConnectorType>(
                Error.Validation(
                    "datasources.type.invalid",
                    "Data source type is invalid."));
        }

        return dataSourceType switch
        {
            DataSourceType.PostgreSql => Result.Success(ConnectorType.PostgreSql),
            DataSourceType.SqlServer => Result.Success(ConnectorType.SqlServer),
            DataSourceType.MySql => Result.Success(ConnectorType.MySql),
            DataSourceType.Sqlite => Result.Success(ConnectorType.Sqlite),
            _ => Result.Failure<ConnectorType>(
                Error.Failure(
                    "datasources.type.unsupported",
                    $"Data source type '{dataSourceType}' is not supported."))
        };
    }

    /// <summary>
    /// Validates a raw connection string value.
    /// </summary>
    /// <param name="connectionString">The raw connection string.</param>
    /// <returns>A result indicating whether the value is valid.</returns>
    private static Result ValidateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Result.Failure(
                Error.Validation(
                    "datasources.connection_string.required",
                    "Connection string is required."));
        }

        return Result.Success();
    }
}