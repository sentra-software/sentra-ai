namespace Sentra.Domain.Users;

/// <summary>
/// Represents the unique identifier of a user.
/// </summary>
public readonly record struct UserId(Guid Value)
{
    /// <summary>
    /// Creates a new user identifier.
    /// </summary>
    /// <returns>A new <see cref="UserId"/> instance.</returns>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>
    /// Returns the string representation of the user identifier.
    /// </summary>
    /// <returns>The identifier as a string.</returns>
    public override string ToString() => Value.ToString();
}