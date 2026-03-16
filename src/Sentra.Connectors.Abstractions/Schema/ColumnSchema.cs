namespace Sentra.Connectors.Abstractions.Schema;

/// <summary>
/// Represents a database column within a discovered table schema.
/// </summary>
/// <param name="Name">The column name.</param>
/// <param name="DataType">The database-specific column data type.</param>
/// <param name="IsNullable">Indicates whether the column allows null values.</param>
public sealed record ColumnSchema(
    string Name,
    string DataType,
    bool IsNullable);