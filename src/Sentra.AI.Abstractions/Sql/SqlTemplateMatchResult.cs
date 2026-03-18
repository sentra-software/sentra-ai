namespace Sentra.AI.Abstractions.Sql;

/// <summary>
/// Represents the result of attempting to match a natural-language question to a predefined SQL template.
/// </summary>
/// <param name="IsMatch">Indicates whether a template match was found.</param>
/// <param name="Sql">The matched SQL query when a match exists; otherwise, <see langword="null"/>.</param>
public sealed record SqlTemplateMatchResult(
    bool IsMatch,
    string? Sql)
{
    /// <summary>
    /// Creates a successful template match result.
    /// </summary>
    /// <param name="sql">The matched SQL query.</param>
    /// <returns>A successful <see cref="SqlTemplateMatchResult"/>.</returns>
    public static SqlTemplateMatchResult Match(string sql)
    {
        return new SqlTemplateMatchResult(true, sql);
    }

    /// <summary>
    /// Creates a result indicating that no template match was found.
    /// </summary>
    /// <returns>A non-matching <see cref="SqlTemplateMatchResult"/>.</returns>
    public static SqlTemplateMatchResult NoMatch()
    {
        return new SqlTemplateMatchResult(false, null);
    }
}