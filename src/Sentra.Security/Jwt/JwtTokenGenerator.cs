using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sentra.Security.Identity;

namespace Sentra.Security.Jwt;

/// <summary>
/// Generates JWT access tokens for authenticated Sentra users.
/// </summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenGenerator"/> class.
    /// </summary>
    /// <param name="options">The JWT options.</param>
    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public string GenerateToken(ApplicationIdentityUser user, IReadOnlyCollection<string> roles)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Email ?? string.Empty),
            new("tenant_id", user.TenantId.ToString()),
            new("domain_user_id", user.DomainUserId.ToString()),
            new("display_name", user.DisplayName)
        ];

        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_options.SigningKey));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        DateTime utcNow = DateTime.UtcNow;

        JwtSecurityToken token = new(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: utcNow,
            expires: utcNow.AddMinutes(_options.ExpirationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}