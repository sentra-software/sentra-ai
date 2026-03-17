using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Represents a configurable test connector used for schema discovery tests.
/// </summary>
internal sealed class TestSchemaDataConnector : IDataConnector
{
    private readonly IReadOnlyCollection<TableSchema> _schema;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestSchemaDataConnector"/> class.
    /// </summary>
    /// <param name="type">The connector type.</param>
    /// <param name="schema">The schema to return.</param>
    public TestSchemaDataConnector(
        ConnectorType type,
        IReadOnlyCollection<TableSchema> schema)
    {
        Type = type;
        _schema = schema;
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
        ConnectorCapability.ReadSchema
    ];

    /// <summary>
    /// Gets the last connection string received through <see cref="ReadSchemaAsync"/>.
    /// </summary>
    public string? LastConnectionString { get; private set; }

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
    /// Returns the configured schema.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The configured schema collection.</returns>
    public Task<IReadOnlyCollection<TableSchema>> ReadSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        LastConnectionString = connectionString;
        return Task.FromResult(_schema);
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