namespace Sentra.Infrastructure.Billing.Stripe;

public sealed class StripeBillingOptions
{
    public const string SectionName = "StripeBilling";

    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;

    public string StarterMonthlyPriceId { get; set; } = string.Empty;
    public string StarterYearlyPriceId { get; set; } = string.Empty;
    public string GrowthMonthlyPriceId { get; set; } = string.Empty;
    public string GrowthYearlyPriceId { get; set; } = string.Empty;
}