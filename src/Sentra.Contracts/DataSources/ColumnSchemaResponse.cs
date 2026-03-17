namespace Sentra.Contracts.DataSources;

/// <summary>
/// Represents a column in a returned table schema response.
/// </summary>
/// <param name="Name">The column name.</param>
/// <param name="DataType">The database-specific column data type.</param>
/// <param name="IsNullable">Indicates whether the column allows null values.</param>
public sealed record ColumnSchemaResponse(
    string Name,
    string DataType,
    bool IsNullable);