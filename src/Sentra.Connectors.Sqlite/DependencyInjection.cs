using Microsoft.Extensions.DependencyInjection;
using Sentra.Connectors.Abstractions.Connectors;

namespace Sentra.Connectors.Sqlite;
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
    public static IServiceCollection AddSentraSqliteConnector(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IDataConnector, SqliteConnector>();

        return services;
    }
}