using Sentra.Domain.DataSources;

namespace Sentra.Api.Models.Chat;

/// <summary>
/// Represents a request to ask a question.
/// </summary>
public sealed record AskQuestionRequest(
    DataSourceType DataSourceType,
    string ConnectionString,
    string Question);