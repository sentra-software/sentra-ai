namespace Sentra.Api.Models;

/// <summary>
/// Represents a standardized API error response.
/// </summary>
/// <param name="Code">The machine-readable error code.</param>
/// <param name="Message">The human-readable error message.</param>
public sealed record ApiErrorResponse(
    string Code,
    string Message);