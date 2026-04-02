using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sentra.Security;
using Sentra.Security.Jwt;
using Sentra.UnitTests.Common;
using Xunit;

namespace Sentra.UnitTests.Security;

/// <summary>
/// Contains tests for security dependency injection registration.
/// </summary>
public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddSentraSecurity_ShouldRegisterJwtTokenGenerator()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var configuration = ConfigurationFactory.CreateJwtConfiguration();

        // Act
        services.AddSentraSecurity(configuration);
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IJwtTokenGenerator>().Should().NotBeNull();
        provider.GetService<IJwtTokenGenerator>().Should().BeOfType<JwtTokenGenerator>();
    }

    [Fact]
    public void AddSentraSecurity_ShouldBindJwtOptions_WhenConfigurationIsValid()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var configuration = ConfigurationFactory.CreateJwtConfiguration(
            issuer: "issuer-value",
            audience: "audience-value",
            signingKey: "signing-key-value-123456789",
            expirationInMinutes: "45");

        // Act
        services.AddSentraSecurity(configuration);
        ServiceProvider provider = services.BuildServiceProvider();
        JwtOptions options = provider.GetRequiredService<IOptions<JwtOptions>>().Value;

        // Assert
        options.Issuer.Should().Be("issuer-value");
        options.Audience.Should().Be("audience-value");
        options.SigningKey.Should().Be("signing-key-value-123456789");
        options.ExpirationInMinutes.Should().Be(45);
    }

    [Fact]
    public void AddSentraSecurity_ShouldReturnSameServiceCollectionInstance()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var configuration = ConfigurationFactory.CreateJwtConfiguration();

        // Act
        IServiceCollection result = services.AddSentraSecurity(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }
}