using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentra.Application.Abstractions.Licensing;
using Sentra.Api.Models;
using Sentra.Security.Extensions;
using Sentra.SharedKernel.Results;

namespace Sentra.Api.Controllers;

/// <summary>
/// Provides access to tenant licensing information.
/// </summary>
[ApiController]
[Authorize]
[Route("api/licensing")]
public sealed class LicensingController : ControllerBase
{
    private readonly ICurrentTenantLicenseService _licenseService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LicensingController"/> class.
    /// </summary>
    public LicensingController(ICurrentTenantLicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    /// <summary>
    /// Gets the current tenant license snapshot.
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        Guid tenantId = User.GetTenantId();
        if (tenantId == Guid.Empty)
        {
            return Unauthorized();
        }

        Result<TenantLicenseSnapshot>? result = await _licenseService.GetSnapshotAsync(tenantId, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                result.Error.Code,
                result.Error.Message));
        }

        return Ok(result.ValueOrThrow());
    }
}