using Sentra.SharedKernel.Results;

namespace Sentra.AI.Abstractions.Chat;

/// <summary>
/// Defines a client for interacting with a chat-capable language model.
/// </summary>
public interface IChatModelClient
{
    /// <summary>
    /// Generates a chat completion from the provided messages.
    /// </summary>
    /// <param name="messages">The chat messages.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the generated chat completion,
    /// or a failed result describing the error.
    /// </returns>
    Task<Result<ChatCompletionResult>> CompleteAsync(
        IReadOnlyCollection<ChatMessage> messages,
        CancellationToken cancellationToken = default);
}