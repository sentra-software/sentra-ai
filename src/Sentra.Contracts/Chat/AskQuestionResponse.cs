namespace Sentra.Api.Models.Chat;

/// <summary>
/// Represents a response for an ask question request.
/// </summary>
public sealed record AskQuestionResponse(
    string Question,
    string GeneratedSql,
    int RowCount,
    string Answer);