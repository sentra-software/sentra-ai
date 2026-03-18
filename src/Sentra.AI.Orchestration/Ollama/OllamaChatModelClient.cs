using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Sentra.AI.Abstractions.Chat;
using Sentra.SharedKernel.Results;

namespace Sentra.AI.Orchestration.Ollama;

/// <summary>
/// Represents a chat model client backed by a local Ollama server.
/// </summary>
public sealed class OllamaChatModelClient : IChatModelClient
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaChatModelClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="options">The Ollama options.</param>
    public OllamaChatModelClient(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options.Value;
    }

    /// <summary>
    /// Generates a chat completion from the provided messages.
    /// </summary>
    /// <param name="messages">The chat messages.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the generated chat completion,
    /// or a failed result describing the error.
    /// </returns>
    public async Task<Result<ChatCompletionResult>> CompleteAsync(
    IReadOnlyCollection<ChatMessage> messages,
    CancellationToken cancellationToken = default)
    {
        if (messages is null || messages.Count == 0)
        {
            return Result.Failure<ChatCompletionResult>(
                Error.Validation(
                    "ai.chat.messages.required",
                    "At least one chat message is required."));
        }

        OllamaChatRequest? request = new OllamaChatRequest(
            _options.Model,
            messages.Select(message => new OllamaChatMessage(message.Role, message.Content)).ToArray(),
            false);

        try
        {
            using HttpResponseMessage? response = await _httpClient.PostAsJsonAsync(
                "/api/chat",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string? responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                return Result.Failure<ChatCompletionResult>(
                    Error.Failure(
                        "ai.chat.request_failed",
                        $"Ollama request failed with status code {(int)response.StatusCode}: {responseBody}"));
            }

            OllamaChatResponse? payload = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
                cancellationToken: cancellationToken);

            if (payload?.Message?.Content is null)
            {
                return Result.Failure<ChatCompletionResult>(
                    Error.Failure(
                        "ai.chat.invalid_response",
                        "Ollama returned an invalid response."));
            }

            return Result.Success(new ChatCompletionResult(payload.Message.Content.Trim()));
        }
        catch (TaskCanceledException)
        {
            return Result.Failure<ChatCompletionResult>(
                Error.Failure(
                    "ai.chat.timeout",
                    "The Ollama request timed out. Verify that Ollama is running and that the configured model is available."));
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<ChatCompletionResult>(
                Error.Failure(
                    "ai.chat.unreachable",
                    $"Could not reach Ollama: {ex.Message}"));
        }
    }

    private sealed record OllamaChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyCollection<OllamaChatMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record OllamaChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaChatResponseMessage? Message);

    private sealed record OllamaChatResponseMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);
}