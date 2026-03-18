namespace Sentra.AI.Abstractions.Chat;

/// <summary>
/// Represents the result of a chat completion request.
/// </summary>
/// <param name="Content">The generated assistant content.</param>
public sealed record ChatCompletionResult(
    string Content);