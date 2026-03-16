namespace Sentra.Domain.Users;

/// <summary>
/// Represents the role of a user within a tenant.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Full access within the tenant context.
    /// </summary>
    Owner = 1,

    /// <summary>
    /// Administrative access within the tenant context.
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Standard application user.
    /// </summary>
    Member = 3,

    /// <summary>
    /// Read-only access for reporting and analysis viewing.
    /// </summary>
    Viewer = 4
}