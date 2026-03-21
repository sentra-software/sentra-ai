using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentra.Application.Abstractions.Auditing;
using Sentra.Application.Abstractions.Billing;
using Sentra.Application.Abstractions.Licensing;
using Sentra.Application.Abstractions.Security;
using Sentra.Infrastructure.Billing.Stripe;
using Sentra.Infrastructure.Licensing;
using Sentra.Infrastructure.Persistence;
using Sentra.Infrastructure.Security;
using Sentra.Security.Identity;

namespace Sentra.Infrastructure;

/// <summary>
/// Provides dependency injection registration for infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSentraInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("SentraPlatform")
            ?? throw new InvalidOperationException("Connection string 'SentraPlatform' was not found.");

        services.AddDbContext<SentraPlatformDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services
            .AddIdentityCore<ApplicationIdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 14;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<ApplicationIdentityRole>()
            .AddEntityFrameworkStores<SentraPlatformDbContext>();

        services.AddDataProtection();
        services.AddScoped<IConnectionStringProtector, DataProtectionConnectionStringProtector>();
        services.AddScoped<IAiQueryAuditLogWriter, AiQueryAuditLogWriter>();
        services.AddScoped<ICurrentTenantLicenseService, CurrentTenantLicenseService>();

        services.Configure<StripeBillingOptions>(options =>
        {
            configuration.GetSection(StripeBillingOptions.SectionName).Bind(options);
        });

        services.AddScoped<IBillingCheckoutService, StripeCheckoutService>();

        return services;
    }
}