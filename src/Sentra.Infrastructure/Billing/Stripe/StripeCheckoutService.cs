using Microsoft.Extensions.Options;
using Sentra.Application.Abstractions.Billing;
using Sentra.SharedKernel.Results;
using Stripe;
using Stripe.Checkout;

namespace Sentra.Infrastructure.Billing.Stripe;

/// <summary>
/// Creates Stripe Checkout Sessions for subscription billing.
/// </summary>
public sealed class StripeCheckoutService : IBillingCheckoutService
{
    private readonly StripeBillingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="StripeCheckoutService"/> class.
    /// </summary>
    public StripeCheckoutService(IOptions<StripeBillingOptions> options)
    {
        _options = options.Value;
        StripeConfiguration.ApiKey = _options.SecretKey;
    }

    /// <inheritdoc />
    public async Task<Result<string>> CreateCheckoutSessionAsync(
        Guid tenantId,
        string email,
        string planCode,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<string>(Error.Validation(
                "billing.checkout.tenant.required",
                "Tenant identifier is required."));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<string>(Error.Validation(
                "billing.checkout.email.required",
                "Email is required."));
        }

        string? priceId = ResolvePriceId(planCode);
        if (string.IsNullOrWhiteSpace(priceId))
        {
            return Result.Failure<string>(Error.Validation(
                "billing.checkout.plan.invalid",
                "The selected plan is not configured."));
        }

        SessionCreateOptions options = new()
        {
            Mode = "subscription",
            SuccessUrl = _options.SuccessUrl,
            CancelUrl = _options.CancelUrl,
            CustomerEmail = email.Trim(),
            Metadata = new Dictionary<string, string>
            {
                ["tenant_id"] = tenantId.ToString(),
                ["plan_code"] = planCode.Trim().ToUpperInvariant()
            },
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["tenant_id"] = tenantId.ToString(),
                    ["plan_code"] = planCode.Trim().ToUpperInvariant()
                }
            },
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                }
            ]
        };

        SessionService sessionService = new();
        Session session = await sessionService.CreateAsync(options, cancellationToken: cancellationToken);

        return Result.Success(session.Url);
    }

    private string? ResolvePriceId(string planCode)
    {
        return planCode.Trim().ToUpperInvariant() switch
        {
            "STARTER" => _options.StarterPriceId,
            "GROWTH" => _options.GrowthPriceId,
            "ENTERPRISE" => _options.EnterprisePriceId,
            _ => null
        };
    }
}