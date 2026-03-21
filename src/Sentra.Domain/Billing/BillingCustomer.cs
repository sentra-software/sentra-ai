using Sentra.Domain.Tenants;
using Sentra.SharedKernel.Abstractions;
using Sentra.SharedKernel.Results;

namespace Sentra.Domain.Billing;

/// <summary>
/// Represents the Stripe customer mapping for a tenant.
/// </summary>
public sealed class BillingCustomer : AggregateRoot<BillingCustomerId>
{
    private BillingCustomer(
        BillingCustomerId id,
        TenantId tenantId,
        string provider,
        string providerCustomerId,
        string email)
        : base(id)
    {
        TenantId = tenantId;
        Provider = provider;
        ProviderCustomerId = providerCustomerId;
        Email = email;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// Gets the billing provider name.
    /// </summary>
    public string Provider { get; }

    /// <summary>
    /// Gets the provider customer identifier.
    /// </summary>
    public string ProviderCustomerId { get; }

    /// <summary>
    /// Gets the billing email.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; }

    /// <summary>
    /// Creates a new billing customer mapping.
    /// </summary>
    public static Result Create(
        TenantId tenantId,
        string provider,
        string providerCustomerId,
        string email)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Result.Failure(Error.Validation(
                "billing.customer.tenant.required",
                "Tenant identifier is required."));
        }

        if (string.IsNullOrWhiteSpace(provider))
        {
            return Result.Failure(Error.Validation(
                "billing.customer.provider.required",
                "Billing provider is required."));
        }

        if (string.IsNullOrWhiteSpace(providerCustomerId))
        {
            return Result.Failure(Error.Validation(
                "billing.customer.provider_customer_id.required",
                "Provider customer identifier is required."));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure(Error.Validation(
                "billing.customer.email.required",
                "Email is required."));
        }

        BillingCustomer customer = new(
            BillingCustomerId.New(),
            tenantId,
            provider.Trim(),
            providerCustomerId.Trim(),
            email.Trim());

        return Result.Success(customer);
    }
}