using Sentra.Application.Abstractions.Connectors;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.DataSources;

/// <summary>
/// Represents the default application service for testing data source connections.
/// </summary>
public sealed class DataSourceConnectionService : IDataSourceConnectionService
{
    private readonly IConnectorRegistry _connectorRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSourceConnectionService"/> class.
    /// </summary>
    /// <param name="connectorRegistry">The connector registry.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="connectorRegistry"/> is <see langword="null"/>.
    /// </exception>
    public DataSourceConnectionService(IConnectorRegistry connectorRegistry)
    {
        ArgumentNullException.ThrowIfNull(connectorRegistry);

        _connectorRegistry = connectorRegistry;
    }

    /// <summary>
    /// Tests a data source connection using the connector that matches the specified data source type.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The raw connection string to test.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the <see cref="ConnectionTestResult"/> when the connector is resolved
    /// and the connection test was executed; otherwise, a failed result describing the error.
    /// </returns>
    public async Task<Result<ConnectionTestResult>> TestConnectionAsync(
        DataSourceType dataSourceType,
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        Result? connectionStringValidationResult = ValidateConnectionString(connectionString);
        if (connectionStringValidationResult.IsFailure)
        {
            return Result.Failure<ConnectionTestResult>(connectionStringValidationResult.Error);
        }

        Result<ConnectorType>? connectorTypeResult = MapToConnectorType(dataSourceType);
        if (connectorTypeResult.IsFailure)
        {
            return Result.Failure<ConnectionTestResult>(connectorTypeResult.Error);
        }

        Result<IDataConnector>? connectorResult = _connectorRegistry.GetConnector(connectorTypeResult.ValueOrThrow());
        if (connectorResult.IsFailure)
        {
            return Result.Failure<ConnectionTestResult>(connectorResult.Error);
        }

        IDataConnector? connector = connectorResult.ValueOrThrow();
        ConnectionTestResult? connectionTestResult = await connector.TestConnectionAsync(
            connectionString.Trim(),
            cancellationToken);

        return Result.Success(connectionTestResult);
    }

    /// <summary>
    /// Maps a domain data source type to a connector type.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <returns>
    /// A successful result containing the mapped <see cref="ConnectorType"/>,
    /// or a failed result when the type is unsupported.
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