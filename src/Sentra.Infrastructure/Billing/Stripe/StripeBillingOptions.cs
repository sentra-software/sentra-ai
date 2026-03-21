namespace Sentra.Infrastructure.Billing.Stripe;

/// <summary>
/// Represents Stripe billing configuration.
/// </summary>
public sealed class StripeBillingOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string SectionName = "StripeBilling";

    /// <summary>
    /// Gets or sets the Stripe secret key.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Stripe webhook signing secret.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the checkout success URL.
    /// </summary>
    public string SuccessUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the checkout cancel URL.
    /// </summary>
    public string CancelUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Stripe price identifier for the Starter plan.
    /// </summary>
    public string StarterPriceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Stripe price identifier for the Growth plan.
    /// </summary>
    public string GrowthPriceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Stripe price identifier for the Enterprise plan.
    /// </summary>
    public string EnterprisePriceId { get; set; } = string.Empty;
}