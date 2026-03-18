namespace Sentra.AI.Orchestration.Ollama;

/// <summary>
/// Represents configuration options for the Ollama client.
/// </summary>
public sealed class OllamaOptions
{
    /// <summary>
    /// Gets or sets the base URL of the Ollama API.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    public string Model { get; set; } = "qwen3:4b";
}