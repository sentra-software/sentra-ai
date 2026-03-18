using Sentra.Application.Abstractions.Connectors;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.DataSources;

/// <summary>
/// Represents the default application service for executing queries against data sources.
/// </summary>
public sealed class DataSourceQueryService : IDataSourceQueryService
{
    private readonly IConnectorRegistry _connectorRegistry;
    private readonly IQuerySafetyValidator _querySafetyValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSourceQueryService"/> class.
    /// </summary>
    /// <param name="connectorRegistry">The connector registry.</param>
    /// <param name="querySafetyValidator">The query safety validator.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="connectorRegistry"/> or <paramref name="querySafetyValidator"/> is <see langword="null"/>.
    /// </exception>
    public DataSourceQueryService(
        IConnectorRegistry connectorRegistry,
        IQuerySafetyValidator querySafetyValidator)
    {
        ArgumentNullException.ThrowIfNull(connectorRegistry);
        ArgumentNullException.ThrowIfNull(querySafetyValidator);

        _connectorRegistry = connectorRegistry;
        _querySafetyValidator = querySafetyValidator;
    }

    /// <summary>
    /// Executes a query using the connector that matches the specified data source type.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The raw connection string.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the <see cref="QueryExecutionResult"/> when the connector is resolved
    /// and the query is executed; otherwise, a failed result describing the error.
    /// </returns>
    public async Task<Result<QueryExecutionResult>> ExecuteQueryAsync(
        DataSourceType dataSourceType,
        string connectionString,
        string query,
        CancellationToken cancellationToken = default)
    {
        Result? connectionStringValidationResult = ValidateConnectionString(connectionString);
        if (connectionStringValidationResult.IsFailure)
        {
            return Result.Failure<QueryExecutionResult>(connectionStringValidationResult.Error);
        }

        Result? querySafetyValidationResult = _querySafetyValidator.Validate(query);
        if (querySafetyValidationResult.IsFailure)
        {
            return Result.Failure<QueryExecutionResult>(querySafetyValidationResult.Error);
        }

        Result<ConnectorType>? connectorTypeResult = MapToConnectorType(dataSourceType);
        if (connectorTypeResult.IsFailure)
        {
            return Result.Failure<QueryExecutionResult>(connectorTypeResult.Error);
        }

        Result<IDataConnector>? connectorResult = _connectorRegistry.GetConnector(connectorTypeResult.ValueOrThrow());
        if (connectorResult.IsFailure)
        {
            return Result.Failure<QueryExecutionResult>(connectorResult.Error);
        }

        IDataConnector? connector = connectorResult.ValueOrThrow();
        QueryExecutionResult? executionResult = await connector.ExecuteQueryAsync(
            connectionString.Trim(),
            query.Trim(),
            cancellationToken);

        return Result.Success(executionResult);
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