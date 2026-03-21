namespace Sentra.Api.Models.Billing;

/// <summary>
/// Represents a request to create a billing checkout session.
/// </summary>
public sealed class CreateCheckoutRequest
{
    /// <summary>
    /// Gets or sets the target plan code.
    /// </summary>
    public string PlanCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target billing interval.
    /// </summary>
    public string BillingInterval { get; set; } = string.Empty;
}