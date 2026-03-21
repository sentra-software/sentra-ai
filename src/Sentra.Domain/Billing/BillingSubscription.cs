using Sentra.Domain.Licensing;
using Sentra.Domain.Tenants;
using Sentra.SharedKernel.Abstractions;
using Sentra.SharedKernel.Results;

namespace Sentra.Domain.Billing;

/// <summary>
/// Represents the Stripe subscription mapping for a tenant.
/// </summary>
public sealed class BillingSubscription : AggregateRoot<BillingSubscriptionId>
{
    private BillingSubscription(
        BillingSubscriptionId id,
        TenantId tenantId,
        LicensePlanId licensePlanId,
        string provider,
        string providerSubscriptionId,
        string status)
        : base(id)
    {
        TenantId = tenantId;
        LicensePlanId = licensePlanId;
        Provider = provider;
        ProviderSubscriptionId = providerSubscriptionId;
        Status = status;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// Gets the license plan identifier.
    /// </summary>
    public LicensePlanId LicensePlanId { get; private set; }

    /// <summary>
    /// Gets the billing provider name.
    /// </summary>
    public string Provider { get; }

    /// <summary>
    /// Gets the provider subscription identifier.
    /// </summary>
    public string ProviderSubscriptionId { get; }

    /// <summary>
    /// Gets the provider status.
    /// </summary>
    public string Status { get; private set; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; }

    /// <summary>
    /// Creates a new billing subscription mapping.
    /// </summary>
    public static Result Create(
        TenantId tenantId,
        LicensePlanId licensePlanId,
        string provider,
        string providerSubscriptionId,
        string status)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Result.Failure(Error.Validation(
                "billing.subscription.tenant.required",
                "Tenant identifier is required."));
        }

        if (licensePlanId.Value == Guid.Empty)
        {
            return Result.Failure(Error.Validation(
                "billing.subscription.plan.required",
                "License plan identifier is required."));
        }

        if (string.IsNullOrWhiteSpace(provider))
        {
            return Result.Failure(Error.Validation(
                "billing.subscription.provider.required",
                "Billing provider is required."));
        }

        if (string.IsNullOrWhiteSpace(providerSubscriptionId))
        {
            return Result.Failure(Error.Validation(
                "billing.subscription.provider_subscription_id.required",
                "Provider subscription identifier is required."));
        }

        BillingSubscription subscription = new(
            BillingSubscriptionId.New(),
            tenantId,
            licensePlanId,
            provider.Trim(),
            providerSubscriptionId.Trim(),
            string.IsNullOrWhiteSpace(status) ? "unknown" : status.Trim());

        return Result.Success(subscription);
    }

    /// <summary>
    /// Updates the provider status.
    /// </summary>
    public void UpdateStatus(string status)
    {
        if (!string.IsNullOrWhiteSpace(status))
        {
            Status = status.Trim();
        }
    }

    /// <summary>
    /// Changes the linked plan.
    /// </summary>
    public void ChangePlan(LicensePlanId licensePlanId)
    {
        LicensePlanId = licensePlanId;
    }
}