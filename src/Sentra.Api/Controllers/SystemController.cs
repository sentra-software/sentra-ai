using Microsoft.AspNetCore.Mvc;

namespace Sentra.Api.Controllers;

/// <summary>
/// Provides system-level endpoints for basic API status information.
/// </summary>
[ApiController]
[Route("")]
public sealed class SystemController : ControllerBase
{
    /// <summary>
    /// Returns a simple response indicating that the Sentra API is running.
    /// </summary>
    /// <returns>A success response containing a status message.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetRoot()
    {
        return Ok("Sentra API is running.");
    }
}