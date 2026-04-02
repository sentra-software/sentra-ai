using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using Sentra.Application.Abstractions.Security;
using Sentra.Infrastructure.Security;

namespace Sentra.UnitTests.Infrastructure.Security;

/// <summary>
/// Contains tests for <see cref="DataProtectionConnectionStringProtector"/>.
/// </summary>
public sealed class DataProtectionConnectionStringProtectorTests
{
    private readonly IConnectionStringProtector _protector;

    public DataProtectionConnectionStringProtectorTests()
    {
        ServiceCollection services = new();
        services.AddDataProtection();

        ServiceProvider provider = services.BuildServiceProvider();
        IDataProtectionProvider dataProtectionProvider = provider.GetRequiredService<IDataProtectionProvider>();

        _protector = new DataProtectionConnectionStringProtector(dataProtectionProvider);
    }

    [Fact]
    public void ProtectAndUnprotect_ShouldRoundTripPlainConnectionString()
    {
        // Arrange
        const string original =
            "Host=localhost;Port=5432;Database=sentra;Username=postgres;Password=secret123!;";

        // Act
        string protectedValue = _protector.Protect(original);
        string unprotectedValue = _protector.Unprotect(protectedValue);

        // Assert
        Assert.NotEqual(original, protectedValue);
        Assert.Equal(original, unprotectedValue);
    }

    [Fact]
    public void Protect_ShouldReturnDifferentCiphertext_ForSameInputAcrossCalls()
    {
        // Arrange
        const string original =
            "Host=localhost;Database=sentra;Username=test;Password=test123!;";

        // Act
        string first = _protector.Protect(original);
        string second = _protector.Protect(original);

        // Assert
        Assert.NotEqual(original, first);
        Assert.NotEqual(original, second);
        Assert.NotEqual(first, second);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Protect_ShouldThrowArgumentException_WhenInputIsEmptyOrWhitespace(string invalidInput)
    {
        // Act + Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _protector.Protect(invalidInput));
        Assert.Equal("plainTextConnectionString", exception.ParamName);
    }

    [Fact]
    public void Protect_ShouldThrowArgumentNullException_WhenInputIsNull()
    {
        // Act + Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => _protector.Protect(null!));
        Assert.Equal("plainTextConnectionString", exception.ParamName);
    }

    [Theory]
    [InlineData("not-encrypted")]
    [InlineData("123")]
    [InlineData("###")]
    public void Unprotect_ShouldThrow_ForInvalidCiphertext(string invalidCipherText)
    {
        // Act + Assert
        Assert.ThrowsAny<Exception>(() => _protector.Unprotect(invalidCipherText));
    }

    [Fact]
    public void Unprotect_ShouldThrowArgumentNullException_WhenInputIsNull()
    {
        // Act + Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => _protector.Unprotect(null!));
        Assert.Equal("protectedConnectionString", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Unprotect_ShouldThrowArgumentException_WhenInputIsEmptyOrWhitespace(string invalidInput)
    {
        // Act + Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _protector.Unprotect(invalidInput));
        Assert.Equal("protectedConnectionString", exception.ParamName);
    }
}