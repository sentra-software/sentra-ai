using Microsoft.AspNetCore.Identity;

namespace Sentra.Security.Identity;

/// <summary>
/// Represents a role within the Sentra platform identity system.
/// </summary>
public sealed class ApplicationIdentityRole : IdentityRole<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationIdentityRole"/> class.
    /// </summary>
    public ApplicationIdentityRole()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationIdentityRole"/> class with the specified role name.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    public ApplicationIdentityRole(string roleName)
        : base(roleName)
    {
    }
}