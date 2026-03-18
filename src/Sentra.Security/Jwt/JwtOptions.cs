namespace Sentra.Security.Jwt;

/// <summary>
/// Represents JWT configuration for the Sentra platform.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Gets or sets the token issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the signing key.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token lifetime in minutes.
    /// </summary>
    public int ExpirationInMinutes { get; set; } = 60;
}