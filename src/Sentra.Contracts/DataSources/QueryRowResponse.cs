namespace Sentra.Contracts.DataSources;

/// <summary>
/// Represents a single query result row.
/// </summary>
/// <param name="Values">The row values keyed by column name.</param>
public sealed record QueryRowResponse(
    IReadOnlyDictionary<string, object?> Values);