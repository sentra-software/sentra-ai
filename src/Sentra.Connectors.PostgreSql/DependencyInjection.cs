using Microsoft.Extensions.DependencyInjection;
using Sentra.Connectors.Abstractions.Connectors;

namespace Sentra.Connectors.PostgreSql;

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
    public static IServiceCollection AddSentraPostgreSqlConnector(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IDataConnector, PostgreSqlConnector>();

        return services;
    }
}