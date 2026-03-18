using Microsoft.AspNetCore.Identity;

namespace Sentra.Security.Identity;

/// <summary>
/// Represents the authentication account for a Sentra platform user.
/// </summary>
public sealed class ApplicationIdentityUser : IdentityUser<Guid>
{
    /// <summary>
    /// Gets or sets the domain user identifier linked to this identity account.
    /// </summary>
    public Guid DomainUserId { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier this identity account belongs to.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the account owner.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the UTC timestamp at which the account was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}