using Microsoft.Extensions.DependencyInjection;
using Sentra.Connectors.Abstractions.Connectors;

namespace Sentra.Connectors.MySql;

/// <summary>
/// Provides dependency injection registration for the Sentra PostgreSQL connector.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Sentra PostgreSQL connector services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSentraMySqlConnector(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IDataConnector, MySqlConnector>();

        return services;
    }
}