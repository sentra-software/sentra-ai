using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.Connectors.Abstractions.Connectors;

/// <summary>
/// Defines a connector that can validate connectivity, discover schema, and execute queries.
/// </summary>
public interface IDataConnector
{
    /// <summary>
    /// Gets the connector type.
    /// </summary>
    ConnectorType Type { get; }

    /// <summary>
    /// Gets the capabilities supported by the connector.
    /// </summary>
    IReadOnlyCollection<ConnectorCapability> Capabilities { get; }

    /// <summary>
    /// Tests the connection using the provided connection string.
    /// </summary>
    /// <param name="connectionString">The raw connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection test result.</returns>
    Task<ConnectionTestResult> TestConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the database schema using the provided connection string.
    /// </summary>
    /// <param name="connectionString">The raw connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of discovered tables.</returns>
    Task<IReadOnlyCollection<TableSchema>> ReadSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a query using the provided connection string.
    /// </summary>
    /// <param name="connectionString">The raw connection string.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query execution result.</returns>
    Task<QueryExecutionResult> ExecuteQueryAsync(
        string connectionString,
        string query,
        CancellationToken cancellationToken = default);
}