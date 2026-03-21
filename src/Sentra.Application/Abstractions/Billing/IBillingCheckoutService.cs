using Sentra.SharedKernel.Results;

namespace Sentra.Application.Abstractions.Billing;

/// <summary>
/// Creates billing checkout sessions.
/// </summary>
public interface IBillingCheckoutService
{
    /// <summary>
    /// Creates a hosted checkout session URL for a tenant and plan.
    /// </summary>
    Task<Result<string>> CreateCheckoutSessionAsync(
        Guid tenantId,
        string email,
        string planCode,
        string billingInterval,
        CancellationToken cancellationToken = default);
}