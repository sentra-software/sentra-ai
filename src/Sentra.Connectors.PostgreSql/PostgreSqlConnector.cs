using Npgsql;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.Connectors.PostgreSql;

/// <summary>
/// Represents the PostgreSQL connector implementation.
/// </summary>
public sealed class PostgreSqlConnector : IDataConnector
{
    /// <summary>
    /// Gets the connector type.
    /// </summary>
    public ConnectorType Type => ConnectorType.PostgreSql;

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
    /// Tests the database connection.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection test result.</returns>
    public async Task<ConnectionTestResult> TestConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using NpgsqlConnection? connection = new NpgsqlConnection(connectionString);

            await connection.OpenAsync(cancellationToken);

            return ConnectionTestResult.Success("Connection succeeded.");
        }
        catch (Exception ex)
        {
            return ConnectionTestResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Reads the PostgreSQL database schema.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The discovered table schemas.</returns>
    public async Task<IReadOnlyCollection<TableSchema>> ReadSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT
                table_schema,
                table_name,
                column_name,
                data_type,
                is_nullable
            FROM information_schema.columns
            WHERE table_schema NOT IN ('pg_catalog','information_schema')
            ORDER BY table_schema, table_name
        """;

        await using NpgsqlConnection? connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using NpgsqlCommand? command = new NpgsqlCommand(query, connection);
        await using NpgsqlDataReader? reader = await command.ExecuteReaderAsync(cancellationToken);

        Dictionary<string, List<ColumnSchema>>? tables = new Dictionary<string, List<ColumnSchema>>();

        while (await reader.ReadAsync(cancellationToken))
        {
            string? schema = reader.GetString(0);
            string? table = reader.GetString(1);
            string? column = reader.GetString(2);
            string? type = reader.GetString(3);
            bool nullable = reader.GetString(4) == "YES";

            string? key = $"{schema}.{table}";

            if (!tables.TryGetValue(key, out List<ColumnSchema>? columns))
            {
                columns = [];
                tables[key] = columns;
            }

            columns.Add(new ColumnSchema(column, type, nullable));
        }

        List<TableSchema>? result = tables
            .Select(t =>
            {
                string[]? split = t.Key.Split('.');

                return new TableSchema(
                    split[0],
                    split[1],
                    t.Value);
            })
            .ToList();

        return result;
    }

    /// <summary>
    /// Executes a SQL query against PostgreSQL.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="query">The SQL query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query execution result.</returns>
    public async Task<QueryExecutionResult> ExecuteQueryAsync(
        string connectionString,
        string query,
        CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection? connection = new NpgsqlConnection(connectionString);

        await connection.OpenAsync(cancellationToken);

        await using NpgsqlCommand? command = new NpgsqlCommand(query, connection);

        await using NpgsqlDataReader? reader = await command.ExecuteReaderAsync(cancellationToken);

        string[]? columns = Enumerable.Range(0, reader.FieldCount)
            .Select(reader.GetName)
            .ToArray();

        List<IReadOnlyDictionary<string, object?>>? rows = new List<IReadOnlyDictionary<string, object?>>();

        while (await reader.ReadAsync(cancellationToken))
        {
            Dictionary<string, object?>? row = new Dictionary<string, object?>();

            foreach (string? column in columns)
            {
                row[column] = reader[column];
            }

            rows.Add(row);
        }

        return new QueryExecutionResult(
            columns,
            rows,
            rows.Count);
    }
}