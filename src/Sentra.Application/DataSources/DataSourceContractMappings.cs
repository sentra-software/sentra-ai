using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Contracts.DataSources;

namespace Sentra.Application.DataSources;

/// <summary>
/// Provides mapping helpers for data source contracts.
/// </summary>
public static class DataSourceContractMappings
{
    /// <summary>
    /// Maps a connection test result to a contract response.
    /// </summary>
    /// <param name="result">The connection test result.</param>
    /// <returns>The mapped response.</returns>
    public static TestDataSourceConnectionResponse ToResponse(this ConnectionTestResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new TestDataSourceConnectionResponse(
            result.IsSuccess,
            result.Message);
    }

    /// <summary>
    /// Maps discovered table schemas to a contract response.
    /// </summary>
    /// <param name="tables">The discovered tables.</param>
    /// <returns>The mapped response.</returns>
    public static ReadDataSourceSchemaResponse ToResponse(this IReadOnlyCollection<TableSchema> tables)
    {
        ArgumentNullException.ThrowIfNull(tables);

        TableSchemaResponse[] mappedTables = tables
            .Select(table => new TableSchemaResponse(
                table.Schema,
                table.Name,
                table.Columns
                    .Select(column => new ColumnSchemaResponse(
                        column.Name,
                        column.DataType,
                        column.IsNullable))
                    .ToArray()))
            .ToArray();

        return new ReadDataSourceSchemaResponse(mappedTables);
    }

    /// <summary>
    /// Maps a query execution result to a contract response.
    /// </summary>
    /// <param name="result">The query execution result.</param>
    /// <returns>The mapped response.</returns>
    public static ExecuteDataSourceQueryResponse ToResponse(this QueryExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        QueryRowResponse[] rows = result.Rows
            .Select(row => new QueryRowResponse(
                new Dictionary<string, object?>(row)))
            .ToArray();

        return new ExecuteDataSourceQueryResponse(
            result.Columns.ToArray(),
            rows,
            result.RowCount);
    }
}