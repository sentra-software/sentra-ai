using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Sentra.Security.Extensions;

/// <summary>
/// Provides helper methods for reading Sentra-specific claims.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the authenticated identity user identifier from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The parsed identifier, or <see cref="Guid.Empty"/> when unavailable.</returns>
    public static Guid GetIdentityUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        string? rawValue =
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
            principal.FindFirstValue("sub");

        return Guid.TryParse(rawValue, out Guid value) ? value : Guid.Empty;
    }

    /// <summary>
    /// Gets the tenant identifier from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The parsed identifier, or <see cref="Guid.Empty"/> when unavailable.</returns>
    public static Guid GetTenantId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        string? rawValue = principal.FindFirstValue("tenant_id");
        return Guid.TryParse(rawValue, out Guid value) ? value : Guid.Empty;
    }

    public static string GetEmail(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value
            ?? string.Empty;
    }
}