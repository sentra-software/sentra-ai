namespace Sentra.Contracts.DataSources;

/// <summary>
/// Represents the response of a data source schema read operation.
/// </summary>
/// <param name="Tables">The discovered tables.</param>
public sealed record ReadDataSourceSchemaResponse(
    IReadOnlyCollection<TableSchemaResponse> Tables);