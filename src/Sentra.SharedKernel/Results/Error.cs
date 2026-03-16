namespace Sentra.SharedKernel.Results;

/// <summary>
/// Represents an application error with a machine-readable code and human-readable message.
/// </summary>
/// <param name="Code">The unique error code.</param>
/// <param name="Message">The human-readable error message.</param>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Gets the empty error instance used for successful results.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Gets a value indicating whether this instance represents no error.
    /// </summary>
    public bool IsNone => this == None;

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    /// <param name="code">The validation error code.</param>
    /// <param name="message">The validation error message.</param>
    /// <returns>A new <see cref="Error"/> instance.</returns>
    public static Error Validation(string code, string message) => new(code, message);

    /// <summary>
    /// Creates a generic failure error.
    /// </summary>
    /// <param name="code">The failure error code.</param>
    /// <param name="message">The failure error message.</param>
    /// <returns>A new <see cref="Error"/> instance.</returns>
    public static Error Failure(string code, string message) => new(code, message);

    /// <summary>
    /// Returns the string representation of the error.
    /// </summary>
    /// <returns>A formatted string containing the code and message.</returns>
    public override string ToString() => $"{Code}: {Message}";
}