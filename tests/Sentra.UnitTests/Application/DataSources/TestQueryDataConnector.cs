using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Represents a configurable test connector used for query execution tests.
/// </summary>
internal sealed class TestQueryDataConnector : IDataConnector
{
    private readonly QueryExecutionResult _queryExecutionResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestQueryDataConnector"/> class.
    /// </summary>
    /// <param name="type">The connector type.</param>
    /// <param name="queryExecutionResult">The query result to return.</param>
    public TestQueryDataConnector(
        ConnectorType type,
        QueryExecutionResult queryExecutionResult)
    {
        Type = type;
        _queryExecutionResult = queryExecutionResult;
    }

    /// <summary>
    /// Gets the connector type.
    /// </summary>
    public ConnectorType Type { get; }

    /// <summary>
    /// Gets the connector capabilities.
    /// </summary>
    public IReadOnlyCollection<ConnectorCapability> Capabilities =>
    [
        ConnectorCapability.ExecuteQuery
    ];

    /// <summary>
    /// Gets the last connection string received through <see cref="ExecuteQueryAsync"/>.
    /// </summary>
    public string? LastConnectionString { get; private set; }

    /// <summary>
    /// Gets the last query received through <see cref="ExecuteQueryAsync"/>.
    /// </summary>
    public string? LastQuery { get; private set; }

    /// <summary>
    /// Returns a successful connection result.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A successful <see cref="ConnectionTestResult"/>.</returns>
    public Task<ConnectionTestResult> TestConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ConnectionTestResult.Success("OK"));
    }

    /// <summary>
    /// Returns an empty schema collection.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty schema collection.</returns>
    public Task<IReadOnlyCollection<TableSchema>> ReadSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<TableSchema> result = Array.Empty<TableSchema>();
        return Task.FromResult(result);
    }

    /// <summary>
    /// Returns the configured query execution result.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="query">The SQL query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The configured <see cref="QueryExecutionResult"/>.</returns>
    public Task<QueryExecutionResult> ExecuteQueryAsync(
        string connectionString,
        string query,
        CancellationToken cancellationToken = default)
    {
        LastConnectionString = connectionString;
        LastQuery = query;

        return Task.FromResult(_queryExecutionResult);
    }
}