using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentra.Security.Jwt;

namespace Sentra.Security;

/// <summary>
/// Provides dependency injection registration for security services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds security services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSentraSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(options =>
        {
            IConfigurationSection section = configuration.GetSection(JwtOptions.SectionName);

            options.Issuer = section["Issuer"] ?? string.Empty;
            options.Audience = section["Audience"] ?? string.Empty;
            options.SigningKey = section["SigningKey"] ?? string.Empty;

            if (int.TryParse(section["ExpirationInMinutes"], out int expirationInMinutes))
            {
                options.ExpirationInMinutes = expirationInMinutes;
            }
        });

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}