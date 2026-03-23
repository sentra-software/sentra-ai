using Microsoft.Data.Sqlite;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.Connectors.Sqlite;

/// <summary>
/// Represents the SQLite connector implementation.
/// </summary>
public sealed class SqliteConnector : IDataConnector
{
    /// <summary>
    /// Gets the connector type.
    /// </summary>
    public ConnectorType Type => ConnectorType.Sqlite;

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
            await using SqliteConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            return ConnectionTestResult.Success("Connection succeeded.");
        }
        catch (Exception ex)
        {
            return ConnectionTestResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Reads the SQLite database schema.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The discovered table schemas.</returns>
    public async Task<IReadOnlyCollection<TableSchema>> ReadSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        const string tablesQuery = """
            SELECT name
            FROM sqlite_master
            WHERE type = 'table'
              AND name NOT LIKE 'sqlite_%'
            ORDER BY name;
            """;

        await using SqliteConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        List<string> tableNames = [];

        await using (SqliteCommand tablesCommand = new(tablesQuery, connection))
        await using (SqliteDataReader tablesReader = await tablesCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await tablesReader.ReadAsync(cancellationToken))
            {
                tableNames.Add(tablesReader.GetString(0));
            }
        }

        List<TableSchema> result = [];

        foreach (string tableName in tableNames)
        {
            string pragmaQuery = $"PRAGMA table_info(\"{tableName.Replace("\"", "\"\"")}\");";

            await using SqliteCommand pragmaCommand = new(pragmaQuery, connection);
            await using SqliteDataReader pragmaReader = await pragmaCommand.ExecuteReaderAsync(cancellationToken);

            List<ColumnSchema> columns = [];

            while (await pragmaReader.ReadAsync(cancellationToken))
            {
                string columnName = pragmaReader.GetString(1);
                string dataType = pragmaReader.IsDBNull(2) ? "TEXT" : pragmaReader.GetString(2);
                bool isNullable = pragmaReader.GetInt32(3) == 0;

                columns.Add(new ColumnSchema(columnName, dataType, isNullable));
            }

            result.Add(new TableSchema("main", tableName, columns));
        }

        return result;
    }

    /// <summary>
    /// Executes a SQL query against SQLite.
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
        await using SqliteConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqliteCommand command = new(query, connection);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

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