using Microsoft.Extensions.DependencyInjection;

namespace Sentra.Api;

/// <summary>
/// Provides dependency injection registration for the Sentra API layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Sentra API services, including controllers and OpenAPI generation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSentraApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddControllers();
        services.AddOpenApi();

        return services;
    }
}