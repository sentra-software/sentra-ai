namespace Sentra.Api.Models.Chat;

/// <summary>
/// Represents a preview response for a generated SQL query.
/// </summary>
public sealed class PreviewQuestionResponse
{
    /// <summary>
    /// Gets or sets the original question.
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated SQL.
    /// </summary>
    public string GeneratedSql { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the query passed safety validation.
    /// </summary>
    public bool IsSafe { get; set; }
}