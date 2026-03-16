namespace Sentra.Domain.DataSources;

/// <summary>
/// Represents the unique identifier of a data source.
/// </summary>
public readonly record struct DataSourceId(Guid Value)
{
    /// <summary>
    /// Creates a new data source identifier.
    /// </summary>
    /// <returns>A new <see cref="DataSourceId"/> instance.</returns>
    public static DataSourceId New() => new(Guid.NewGuid());

    /// <summary>
    /// Returns the string representation of the identifier.
    /// </summary>
    /// <returns>The identifier as a string.</returns>
    public override string ToString() => Value.ToString();
}