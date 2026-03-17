namespace Sentra.Contracts.DataSources;

/// <summary>
/// Represents the response of a data source query execution.
/// </summary>
/// <param name="Columns">The returned columns in result order.</param>
/// <param name="Rows">The returned rows.</param>
/// <param name="RowCount">The total number of returned rows.</param>
public sealed record ExecuteDataSourceQueryResponse(
    IReadOnlyCollection<string> Columns,
    IReadOnlyCollection<QueryRowResponse> Rows,
    int RowCount);