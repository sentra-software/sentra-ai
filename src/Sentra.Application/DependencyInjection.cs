using Microsoft.Extensions.DependencyInjection;
using Sentra.Application.Abstractions.Connectors;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Application.Connectors;
using Sentra.Application.DataSources;

namespace Sentra.Application;

/// <summary>
/// Provides dependency injection registration for the Sentra application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Sentra application layer services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSentraApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IConnectorRegistry, ConnectorRegistry>();

        services.AddScoped<IDataSourceConnectionService, DataSourceConnectionService>();
        services.AddScoped<IDataSourceSchemaService, DataSourceSchemaService>();
        services.AddScoped<IDataSourceQueryService, DataSourceQueryService>();

        return services;
    }
}