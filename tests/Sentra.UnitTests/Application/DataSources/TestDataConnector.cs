using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Represents a configurable test connector used for application-layer data source tests.
/// </summary>
internal sealed class TestDataConnector : IDataConnector
{
    private readonly ConnectionTestResult _connectionTestResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataConnector"/> class.
    /// </summary>
    /// <param name="type">The connector type.</param>
    /// <param name="connectionTestResult">The connection test result to return.</param>
    public TestDataConnector(
        ConnectorType type,
        ConnectionTestResult connectionTestResult)
    {
        Type = type;
        _connectionTestResult = connectionTestResult;
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
        ConnectorCapability.TestConnection
    ];

    /// <summary>
    /// Gets the last connection string received through <see cref="TestConnectionAsync"/>.
    /// </summary>
    public string? LastConnectionString { get; private set; }

    /// <summary>
    /// Tests the connection asynchronously.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The configured <see cref="ConnectionTestResult"/>.</returns>
    public Task<ConnectionTestResult> TestConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        LastConnectionString = connectionString;
        return Task.FromResult(_connectionTestResult);
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
    /// Returns an empty query execution result.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="query">The SQL query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty query execution result.</returns>
    public Task<QueryExecutionResult> ExecuteQueryAsync(
        string connectionString,
        string query,
        CancellationToken cancellationToken = default)
    {
        QueryExecutionResult? result = new QueryExecutionResult(
            Array.Empty<string>(),
            Array.Empty<IReadOnlyDictionary<string, object?>>(),
            0);

        return Task.FromResult(result);
    }
}