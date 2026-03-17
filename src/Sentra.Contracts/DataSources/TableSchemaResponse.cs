namespace Sentra.Contracts.DataSources;

/// <summary>
/// Represents a database table schema in a response.
/// </summary>
/// <param name="Schema">The schema name.</param>
/// <param name="Name">The table name.</param>
/// <param name="Columns">The table columns.</param>
public sealed record TableSchemaResponse(
    string Schema,
    string Name,
    IReadOnlyCollection<ColumnSchemaResponse> Columns);