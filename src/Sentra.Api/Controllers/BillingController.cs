using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentra.Api.Models;
using Sentra.Api.Models.Billing;
using Sentra.Application.Abstractions.Billing;
using Sentra.Security.Extensions;
using Sentra.SharedKernel.Results;

namespace Sentra.Api.Controllers;

/// <summary>
/// Provides billing endpoints for the current tenant.
/// </summary>
[ApiController]
[Authorize]
[Route("api/billing")]
public sealed class BillingController : ControllerBase
{
    private readonly IBillingCheckoutService _billingCheckoutService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillingController"/> class.
    /// </summary>
    public BillingController(IBillingCheckoutService billingCheckoutService)
    {
        _billingCheckoutService = billingCheckoutService;
    }

    /// <summary>
    /// Creates a hosted checkout session for the selected plan.
    /// </summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(CreateCheckoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCheckout(
        [FromBody] CreateCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        Guid tenantId = User.GetTenantId();
        string email = User.FindFirst("email")?.Value ?? string.Empty;

        if (tenantId == Guid.Empty)
        {
            return Unauthorized();
        }

        Result<string>? result = await _billingCheckoutService.CreateCheckoutSessionAsync(
            tenantId,
            email,
            request.PlanCode,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                result.Error.Code,
                result.Error.Message));
        }

        return Ok(new CreateCheckoutResponse
        {
            CheckoutUrl = result.ValueOrThrow()
        });
    }
}