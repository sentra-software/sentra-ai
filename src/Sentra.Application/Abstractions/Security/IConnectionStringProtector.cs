namespace Sentra.Application.Abstractions.Security;

/// <summary>
/// Defines encryption behavior for sensitive connection strings.
/// </summary>
public interface IConnectionStringProtector
{
    /// <summary>
    /// Protects the specified plaintext connection string.
    /// </summary>
    /// <param name="plainTextConnectionString">The plaintext connection string.</param>
    /// <returns>The protected value.</returns>
    string Protect(string plainTextConnectionString);

    /// <summary>
    /// Unprotects the specified protected connection string.
    /// </summary>
    /// <param name="protectedConnectionString">The protected value.</param>
    /// <returns>The plaintext connection string.</returns>
    string Unprotect(string protectedConnectionString);
}