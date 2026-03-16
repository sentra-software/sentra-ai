namespace Sentra.Connectors.Abstractions.Querying;

/// <summary>
/// Represents the result of executing a query through a connector.
/// </summary>
/// <param name="Columns">The returned column names in result order.</param>
/// <param name="Rows">The returned rows where each row is represented as a dictionary.</param>
/// <param name="RowCount">The total number of returned rows.</param>
public sealed record QueryExecutionResult(
    IReadOnlyCollection<string> Columns,
    IReadOnlyCollection<IReadOnlyDictionary<string, object?>> Rows,
    int RowCount);