using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sentra.Infrastructure.Billing.Stripe;
using Stripe;

namespace Sentra.Api.Controllers;

/// <summary>
/// Receives Stripe webhook events.
/// </summary>
[ApiController]
[Route("api/billing/webhooks/stripe")]
public sealed class StripeWebhooksController : ControllerBase
{
    private readonly StripeBillingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="StripeWebhooksController"/> class.
    /// </summary>
    public StripeWebhooksController(IOptions<StripeBillingOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Handles incoming Stripe webhook events.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(cancellationToken);
        string signatureHeader = Request.Headers["Stripe-Signature"]!;

        Event stripeEvent = EventUtility.ConstructEvent(
            json,
            signatureHeader,
            _options.WebhookSecret);

        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
            case "customer.subscription.created":
            case "customer.subscription.updated":
            case "customer.subscription.deleted":
                break;
        }

        return Ok();
    }
}