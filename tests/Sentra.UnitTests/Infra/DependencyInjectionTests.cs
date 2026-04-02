using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sentra.Application.Abstractions.Auditing;
using Sentra.Application.Abstractions.Billing;
using Sentra.Application.Abstractions.Licensing;
using Sentra.Application.Abstractions.Security;
using Sentra.Infrastructure;
using Sentra.Infrastructure.Billing.Stripe;
using Sentra.Infrastructure.Persistence;
using Sentra.Security.Identity;
using Sentra.UnitTests.Common;
using Xunit;

namespace Sentra.UnitTests.Infrastructure;

/// <summary>
/// Contains tests for infrastructure dependency injection registration.
/// </summary>
public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddSentraInfrastructure_ShouldNotThrow_WhenConnectionStringIsMissing()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        IConfiguration? configuration = ConfigurationFactory.CreateInfrastructureConfiguration(
            connectionString: string.Empty);

        // Act
        Action act = () => services.AddSentraInfrastructure(configuration);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddSentraInfrastructure_ShouldRegisterCoreServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration? configuration = ConfigurationFactory.CreateInfrastructureConfiguration();

        // Act
        services.AddSentraInfrastructure(configuration);
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<SentraPlatformDbContext>().Should().NotBeNull();
        provider.GetService<IConnectionStringProtector>().Should().NotBeNull();
        provider.GetService<IAiQueryAuditLogWriter>().Should().NotBeNull();
        provider.GetService<ICurrentTenantLicenseService>().Should().NotBeNull();
        provider.GetService<IBillingCheckoutService>().Should().NotBeNull();
        provider.GetService<IStripeWebhookService>().Should().NotBeNull();
    }

    [Fact]
    public void AddSentraInfrastructure_ShouldBindStripeOptions()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration? configuration = ConfigurationFactory.CreateInfrastructureConfiguration(
            stripeSecretKey: "sk_test_123",
            stripeWebhookSecret: "whsec_456",
            starterMonthlyPriceId: "price_789");

        // Act
        services.AddSentraInfrastructure(configuration);
        ServiceProvider provider = services.BuildServiceProvider();
        StripeBillingOptions options = provider.GetRequiredService<IOptions<StripeBillingOptions>>().Value;

        // Assert
        options.SecretKey.Should().Be("sk_test_123");
        options.WebhookSecret.Should().Be("whsec_456");
        options.StarterMonthlyPriceId.Should().Be("price_789");
    }

    [Fact]
    public void AddSentraInfrastructure_ShouldConfigureIdentityOptions()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration? configuration = ConfigurationFactory.CreateInfrastructureConfiguration();

        // Act
        services.AddSentraInfrastructure(configuration);
        ServiceProvider provider = services.BuildServiceProvider();
        IdentityOptions options = provider.GetRequiredService<IOptions<IdentityOptions>>().Value;

        // Assert
        options.User.RequireUniqueEmail.Should().BeTrue();
        options.Password.RequiredLength.Should().BeGreaterThan(0);
        options.Lockout.AllowedForNewUsers.Should().BeTrue();
    }

    [Fact]
    public void AddSentraInfrastructure_ShouldRegisterDbContextOptions()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration? configuration = ConfigurationFactory.CreateInfrastructureConfiguration();

        // Act
        services.AddSentraInfrastructure(configuration);
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<DbContextOptions<SentraPlatformDbContext>>().Should().NotBeNull();
    }
}