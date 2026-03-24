using Microsoft.Data.SqlClient;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.Connectors.SqlServer;

/// <summary>
/// Represents the SQL Server connector implementation.
/// </summary>
public sealed class SqlServerConnector : IDataConnector
{
    /// <summary>
    /// Gets the connector type.
    /// </summary>
    public ConnectorType Type => ConnectorType.SqlServer;

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
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            return ConnectionTestResult.Success("Connection succeeded.");
        }
        catch (Exception ex)
        {
            return ConnectionTestResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Reads the SQL Server database schema.
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
                c.TABLE_SCHEMA,
                c.TABLE_NAME,
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE
            FROM INFORMATION_SCHEMA.COLUMNS c
            INNER JOIN INFORMATION_SCHEMA.TABLES t
                ON c.TABLE_SCHEMA = t.TABLE_SCHEMA
               AND c.TABLE_NAME = t.TABLE_NAME
            WHERE t.TABLE_TYPE = 'BASE TABLE'
            ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        Dictionary<string, List<ColumnSchema>> tables = new(StringComparer.OrdinalIgnoreCase);

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
            })
            .ToList();

        return result;
    }

    /// <summary>
    /// Executes a SQL query against SQL Server.
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
        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(query, connection);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

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