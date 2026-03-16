using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.UnitTests.Application.Connectors;

/// <summary>
/// Represents a lightweight test connector implementation for application-layer tests.
/// </summary>
internal sealed class TestDataConnector : IDataConnector
{
    private readonly ConnectorType _type;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataConnector"/> class.
    /// </summary>
    /// <param name="type">The connector type exposed by the test connector.</param>
    public TestDataConnector(ConnectorType type)
    {
        _type = type;
    }

    /// <summary>
    /// Gets the connector type.
    /// </summary>
    public ConnectorType Type => _type;

    /// <summary>
    /// Gets the connector capabilities.
    /// </summary>
    public IReadOnlyCollection<ConnectorCapability> Capabilities =>
    [
        ConnectorCapability.TestConnection,
        ConnectorCapability.ReadSchema,
        ConnectorCapability.ExecuteQuery
    ];

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
    /// Returns an empty schema result.
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
    /// Returns an empty query result.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="query">The SQL query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty <see cref="QueryExecutionResult"/>.</returns>
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