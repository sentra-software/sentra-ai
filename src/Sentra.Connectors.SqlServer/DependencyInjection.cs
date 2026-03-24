using Microsoft.Extensions.DependencyInjection;
using Sentra.Connectors.Abstractions.Connectors;

namespace Sentra.Connectors.SqlServer;

/// <summary>
/// Provides dependency injection registration for the Sentra SQL Server connector.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Sentra SQL Server connector services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSentraSqlServerConnector(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IDataConnector, SqlServerConnector>();

        return services;
    }
}