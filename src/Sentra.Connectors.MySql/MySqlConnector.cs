using MySqlConnector;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.Connectors.MySql;

/// <summary>
/// Represents the MySQL connector implementation.
/// </summary>
public sealed class MySqlConnector : IDataConnector
{
    /// <summary>
    /// Gets the connector type.
    /// </summary>
    public ConnectorType Type => ConnectorType.MySql;

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
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await using MySqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            return ConnectionTestResult.Success("Connection succeeded.");
        }
        catch (Exception ex)
        {
            return ConnectionTestResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Reads the MySQL database schema.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The discovered table schemas.</returns>
    public async Task<IReadOnlyCollection<TableSchema>> ReadSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default
    )
    {
        const string query = """
            SELECT
                TABLE_SCHEMA,
                TABLE_NAME,
                COLUMN_NAME,
                COLUMN_TYPE,
                IS_NULLABLE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
            ORDER BY TABLE_SCHEMA, TABLE_NAME, ORDINAL_POSITION;
            """;

        await using MySqlConnection connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using MySqlCommand command = new MySqlCommand(query, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        Dictionary<string, List<ColumnSchema>> tables = new Dictionary<string, List<ColumnSchema>>(StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync(cancellationToken))
        {
            string schema = reader.GetString(0);
            string table = reader.GetString(1);
            string column = reader.GetString(2);
            string type = reader.GetString(3);
            bool nullable = string.Equals(reader.GetString(4), "YES", StringComparison.OrdinalIgnoreCase);

            string key = $"{schema}.{table}";

            if (!tables.TryGetValue(key, out List<ColumnSchema>? columns))
            {
                columns = [];
                tables[key] = columns;
            }

            columns.Add(new ColumnSchema(column, type, nullable));
        }

        List<TableSchema> result = tables
            .Select(tableEntry =>
            {
                string[] parts = tableEntry.Key.Split('.', 2, StringSplitOptions.TrimEntries);
                return new TableSchema(parts[0], parts[1], tableEntry.Value);
            }).ToList();

        return result;
    }

    /// <summary>
    /// Executes a SQL query against MySQL.
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
        await using MySqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using MySqlCommand command = new(query, connection);
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        string[] columns = Enumerable
            .Range(0, reader.FieldCount)
            .Select(reader.GetName)
            .ToArray();

        List<IReadOnlyDictionary<string, object?>> rows = [];

        while (await reader.ReadAsync(cancellationToken))
        {
            Dictionary<string, object?> row = new(StringComparer.OrdinalIgnoreCase);

            foreach (string column in columns)
            {
                object value = reader[column];
                row[column] = value is DBNull ? null : value;
            }

            rows.Add(row);
        }

        return new QueryExecutionResult(columns, rows, rows.Count);
    }
}