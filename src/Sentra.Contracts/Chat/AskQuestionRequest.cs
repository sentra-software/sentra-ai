namespace Sentra.Api.Models.Chat;

/// <summary>
/// Represents a request to ask a natural-language question against a managed data source.
/// </summary>
public sealed class AskQuestionRequest
{
    /// <summary>
    /// Gets or sets the managed data source identifier.
    /// When omitted, the active data source of the current user is used.
    /// </summary>
    public Guid? DataSourceId { get; set; }

    /// <summary>
    /// Gets or sets the natural-language question.
    /// </summary>
    public string Question { get; set; } = string.Empty;
}