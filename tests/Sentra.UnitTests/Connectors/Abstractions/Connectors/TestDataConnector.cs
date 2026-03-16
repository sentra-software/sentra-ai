using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.UnitTests.Connectors.Abstractions.Connectors;

/// <summary>
/// Represents a test implementation of <see cref="IDataConnector"/>.
/// </summary>
internal sealed class TestDataConnector : IDataConnector
{
    /// <summary>
    /// Gets the connector type.
    /// </summary>
    public ConnectorType Type => ConnectorType.PostgreSql;

    /// <summary>
    /// Gets the connector capabilities.
    /// </summary>
    public IReadOnlyCollection<ConnectorCapability> Capabilities =>
        new[]
        {
            ConnectorCapability.TestConnection,
            ConnectorCapability.ReadSchema,
            ConnectorCapability.ExecuteQuery
        };

    /// <summary>
    /// Tests the connection asynchronously.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A successful connection test result.</returns>
    public Task<ConnectionTestResult> TestConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ConnectionTestResult.Success("OK"));
    }

    /// <summary>
    /// Reads the schema asynchronously.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A test table schema collection.</returns>
    public Task<IReadOnlyCollection<TableSchema>> ReadSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<TableSchema> result =
        [
            new TableSchema(
                "public",
                "customers",
                new[]
                {
                    new ColumnSchema("id", "uuid", false)
                })
        ];

        return Task.FromResult(result);
    }

    /// <summary>
    /// Executes a query asynchronously.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A test query execution result.</returns>
    public Task<QueryExecutionResult> ExecuteQueryAsync(
        string connectionString,
        string query,
        CancellationToken cancellationToken = default)
    {
        QueryExecutionResult? result = new QueryExecutionResult(
            new[] { "id" },
            new[]
            {
                new Dictionary<string, object?>
                {
                    ["id"] = 1
                }
            },
            1);

        return Task.FromResult(result);
    }
}