using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sentra.Application.Abstractions.Billing;
using Sentra.Infrastructure.Billing.Stripe;
using Stripe;
using Stripe.Checkout;
using StripeSubscription = Stripe.Subscription;

namespace Sentra.Api.Controllers;

/// <summary>
/// Receives Stripe webhook events.
/// </summary>
[ApiController]
[Route("api/billing/webhooks/stripe")]
public sealed class StripeWebhooksController : ControllerBase
{
    private readonly StripeBillingOptions _options;
    private readonly IStripeWebhookService _stripeWebhookService;
    private readonly ILogger<StripeWebhooksController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StripeWebhooksController"/> class.
    /// </summary>
    public StripeWebhooksController(
        IOptions<StripeBillingOptions> options,
        IStripeWebhookService stripeWebhookService,
        ILogger<StripeWebhooksController> logger)
    {
        _options = options.Value;
        _stripeWebhookService = stripeWebhookService;
        _logger = logger;
    }

    /// <summary>
    /// Handles incoming Stripe webhook events.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        string json = await new StreamReader(HttpContext.Request.Body)
            .ReadToEndAsync(cancellationToken);

        string signatureHeader = Request.Headers["Stripe-Signature"]!;

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                signatureHeader,
                _options.WebhookSecret);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid Stripe webhook signature.");
            return BadRequest();
        }

        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
                {
                    Session? session = stripeEvent.Data.Object as Session;
                    if (session is null)
                    {
                        return BadRequest();
                    }

                    await _stripeWebhookService.HandleCheckoutCompletedAsync(
                        session,
                        cancellationToken);

                    break;
                }

            case "customer.subscription.updated":
                {
                    StripeSubscription? subscription = stripeEvent.Data.Object as StripeSubscription;
                    if (subscription is null)
                    {
                        return BadRequest();
                    }

                    await _stripeWebhookService.HandleSubscriptionUpdatedAsync(
                        subscription,
                        cancellationToken);

                    break;
                }

            case "customer.subscription.deleted":
                {
                    StripeSubscription? subscription = stripeEvent.Data.Object as StripeSubscription;
                    if (subscription is null)
                    {
                        return BadRequest();
                    }

                    await _stripeWebhookService.HandleSubscriptionDeletedAsync(
                        subscription,
                        cancellationToken);

                    break;
                }

            default:
                _logger.LogInformation(
                    "Unhandled Stripe webhook event type: {EventType}",
                    stripeEvent.Type);
                break;
        }

        return Ok();
    }
}