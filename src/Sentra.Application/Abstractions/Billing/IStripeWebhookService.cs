using Stripe.Checkout;
using StripeSubscription = Stripe.Subscription;

namespace Sentra.Application.Abstractions.Billing;

/// <summary>
/// Handles Stripe webhook events and synchronizes billing state into Sentra.
/// </summary>
public interface IStripeWebhookService
{
    /// <summary>
    /// Handles a completed Stripe Checkout session.
    /// </summary>
    Task HandleCheckoutCompletedAsync(
        Session session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles a Stripe subscription update.
    /// </summary>
    Task HandleSubscriptionUpdatedAsync(
        StripeSubscription subscription,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles a Stripe subscription deletion.
    /// </summary>
    Task HandleSubscriptionDeletedAsync(
        StripeSubscription subscription,
        CancellationToken cancellationToken = default);
}