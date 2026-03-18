namespace Sentra.AI.Abstractions.Chat;

/// <summary>
/// Represents a chat message exchanged with a language model.
/// </summary>
/// <param name="Role">The message role, such as system, user, or assistant.</param>
/// <param name="Content">The message content.</param>
public sealed record ChatMessage(
    string Role,
    string Content
);