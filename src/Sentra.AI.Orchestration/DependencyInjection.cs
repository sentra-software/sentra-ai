using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentra.AI.Abstractions.Answers;
using Sentra.AI.Abstractions.Chat;
using Sentra.AI.Abstractions.Sql;
using Sentra.AI.Orchestration.Answers;
using Sentra.AI.Orchestration.Ollama;
using Sentra.AI.Orchestration.Sql;
using Sentra.Application.Abstractions.DataSources;

namespace Sentra.AI.Orchestration;

/// <summary>
/// Provides dependency injection registration for the Sentra AI orchestration layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Sentra AI orchestration services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSentraAiOrchestration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<OllamaOptions>(configuration.GetSection("Ollama"));

        services.AddHttpClient<IChatModelClient, OllamaChatModelClient>((serviceProvider, httpClient) =>
        {
            OllamaOptions? options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>()
                .Value;

            httpClient.BaseAddress = new Uri(options.BaseUrl);
            httpClient.Timeout = TimeSpan.FromMinutes(5);
        });

        services.AddScoped<ISqlTemplateMatcher, SqlTemplateMatcher>();
        services.AddScoped<ISqlIdentifierNormalizer, SqlIdentifierNormalizer>();
        services.AddScoped<ISqlGenerationService, SqlGenerationService>();
        services.AddScoped<IAnswerGenerationService, AnswerGenerationService>();

        return services;
    }
}