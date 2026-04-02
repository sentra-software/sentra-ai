using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentra.Infrastructure;
using Sentra.Security;

namespace Sentra.IntegrationTests.Common;

/// <summary>
/// Creates service scopes for integration tests.
/// </summary>
public sealed class TestServiceScopeFactory : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestServiceScopeFactory"/> class.
    /// </summary>
    public TestServiceScopeFactory()
    {
        IServiceCollection services = new ServiceCollection();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SentraPlatform"] = "Host=localhost;Port=5432;Database=sentra_integration;Username=test;Password=test123!",
                ["Jwt:Issuer"] = "sentra-integration",
                ["Jwt:Audience"] = "sentra-api",
                ["Jwt:SigningKey"] = "integration-signing-key-1234567890",
                ["Jwt:ExpirationInMinutes"] = "60",
                ["Stripe:SecretKey"] = "sk_test_default",
                ["Stripe:WebhookSecret"] = "whsec_default",
                ["Stripe:StarterPlanPriceId"] = "price_default"
            })
            .Build();

        services.AddSentraSecurity(configuration);
        services.AddSentraInfrastructure(configuration);

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates a new service scope.
    /// </summary>
    public IServiceScope CreateScope()
    {
        return _serviceProvider.CreateScope();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
}