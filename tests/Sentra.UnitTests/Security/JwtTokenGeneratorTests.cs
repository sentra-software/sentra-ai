using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using FluentAssertions;

using Microsoft.Extensions.Options;

using Sentra.Security.Identity;
using Sentra.Security.Jwt;
using Xunit;

namespace Sentra.UnitTests.Security;

/// <summary>
/// Contains tests for <see cref="JwtTokenGenerator"/>.
/// </summary>
public sealed class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        JwtOptions options = new()
        {
            Issuer = "sentra-test-issuer",
            Audience = "sentra-test-audience",
            SigningKey = "super-secure-signing-key-for-tests-123456789",
            ExpirationInMinutes = 60
        };

        _generator = new JwtTokenGenerator(Options.Create(options));
    }

    [Fact]
    public void GenerateToken_ShouldReturnSerializedJwt()
    {
        // Arrange
        ApplicationIdentityUser user = CreateUser();
        string[] roles = ["Owner", "Admin"];
        string platformRole = "PlatformOwner";

        // Act
        string token = _generator.GenerateToken(user, roles, platformRole);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ShouldContainExpectedIdentityClaims()
    {
        // Arrange
        ApplicationIdentityUser user = CreateUser();
        string[] roles = ["Owner", "Admin"];
        string platformRole = "PlatformOwner";

        // Act
        string token = _generator.GenerateToken(user, roles, platformRole);

        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.Should().Contain(c => c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Value == user.Email);
    }

    [Fact]
    public void GenerateToken_ShouldContainRoles()
    {
        // Arrange
        ApplicationIdentityUser user = CreateUser();
        string[] roles = ["Owner", "Admin"];
        string platformRole = "PlatformOwner";

        // Act
        string token = _generator.GenerateToken(user, roles, platformRole);

        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .Should()
            .Contain(["Owner", "Admin"]);
    }

    [Fact]
    public void GenerateToken_ShouldUseConfiguredIssuerAndAudience()
    {
        // Arrange
        ApplicationIdentityUser user = CreateUser();

        // Act
        string token = _generator.GenerateToken(user, ["Owner"], "PlatformOwner");

        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.Issuer.Should().Be("sentra-test-issuer");
        jwt.Audiences.Should().Contain("sentra-test-audience");
    }

    [Fact]
    public void GenerateToken_ShouldSetFutureExpiration()
    {
        // Arrange
        ApplicationIdentityUser user = CreateUser();
        DateTime before = DateTime.UtcNow;

        // Act
        string token = _generator.GenerateToken(user, ["Owner"], "PlatformOwner");

        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jwt.ValidTo.Should().BeAfter(before);
    }

    [Fact]
    public void GenerateToken_ShouldThrow_WhenUserIsNull()
    {
        // Act
        Action act = () => _generator.GenerateToken(null!, ["Owner"], "PlatformOwner");

        // Assert
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void GenerateToken_ShouldThrow_WhenRolesIsNull()
    {
        // Arrange
        ApplicationIdentityUser user = CreateUser();

        // Act
        Action act = () => _generator.GenerateToken(user, null!, "PlatformOwner");

        // Assert
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void GenerateToken_ShouldThrow_WhenPlatformRoleIsNull()
    {
        // Arrange
        ApplicationIdentityUser user = CreateUser();

        // Act
        Action act = () => _generator.GenerateToken(user, ["Owner"], null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private static ApplicationIdentityUser CreateUser()
    {
        return new ApplicationIdentityUser
        {
            Id = Guid.NewGuid(),
            UserName = "josey",
            Email = "josey@sentra.dev"
        };
    }
}