using Sentra.Security.Identity;

namespace Sentra.Security.Jwt;

/// <summary>
/// Defines JWT token generation behavior.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a signed JWT access token for the specified identity user.
    /// </summary>
    /// <param name="user">The identity user.</param>
    /// <param name="roles">The assigned roles.</param>
    /// <returns>The serialized JWT access token.</returns>
    string GenerateToken(ApplicationIdentityUser user, IReadOnlyCollection<string> roles);
}