namespace Sentra.Connectors.Abstractions.Schema;

/// <summary>
/// Represents a discovered database table schema.
/// </summary>
/// <param name="Schema">The database schema name.</param>
/// <param name="Name">The table name.</param>
/// <param name="Columns">The columns belonging to the table.</param>
public sealed record TableSchema(
    string Schema,
    string Name,
    IReadOnlyCollection<ColumnSchema> Columns);