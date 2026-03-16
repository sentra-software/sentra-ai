namespace Sentra.Connectors.Abstractions.Connectors;

/// <summary>
/// Represents the result of testing a connector connection.
/// </summary>
/// <param name="IsSuccess">Indicates whether the connection test succeeded.</param>
/// <param name="Message">The connection test message.</param>
public sealed record ConnectionTestResult(
    bool IsSuccess,
    string Message)
{
    /// <summary>
    /// Creates a successful connection test result.
    /// </summary>
    /// <param name="message">The success message.</param>
    /// <returns>A successful <see cref="ConnectionTestResult"/>.</returns>
    public static ConnectionTestResult Success(string message)
    {
        return new ConnectionTestResult(true, message);
    }

    /// <summary>
    /// Creates a failed connection test result.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed <see cref="ConnectionTestResult"/>.</returns>
    public static ConnectionTestResult Failure(string message)
    {
        return new ConnectionTestResult(false, message);
    }
}