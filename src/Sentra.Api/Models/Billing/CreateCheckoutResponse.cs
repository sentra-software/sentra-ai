namespace Sentra.Api.Models.Billing;

/// <summary>
/// Represents a billing checkout session response.
/// </summary>
public sealed class CreateCheckoutResponse
{
    /// <summary>
    /// Gets or sets the hosted checkout URL.
    /// </summary>
    public string CheckoutUrl { get; set; } = string.Empty;
}