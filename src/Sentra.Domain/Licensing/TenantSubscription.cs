using Sentra.Domain.Tenants;
using Sentra.SharedKernel.Abstractions;
using Sentra.SharedKernel.Results;

namespace Sentra.Domain.Licensing;

/// <summary>
/// Represents the active license subscription of a tenant.
/// </summary>
public sealed class TenantSubscription : AggregateRoot<TenantSubscriptionId>
{
    private TenantSubscription(
        TenantSubscriptionId id,
        TenantId tenantId,
        LicensePlanId licensePlanId,
        SubscriptionStatus status,
        DateTime startsAtUtc,
        DateTime? endsAtUtc,
        DateTime? trialEndsAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        LicensePlanId = licensePlanId;
        Status = status;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        TrialEndsAtUtc = trialEndsAtUtc;
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
    /// Gets the subscription status.
    /// </summary>
    public SubscriptionStatus Status { get; private set; }

    /// <summary>
    /// Gets the UTC start timestamp.
    /// </summary>
    public DateTime StartsAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC end timestamp.
    /// </summary>
    public DateTime? EndsAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC trial end timestamp.
    /// </summary>
    public DateTime? TrialEndsAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; }

    /// <summary>
    /// Creates a new tenant subscription.
    /// </summary>
    public static Result CreateTrial(
        TenantId tenantId,
        LicensePlanId licensePlanId,
        int trialDays)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Result.Failure(Error.Validation(
                "licensing.subscription.tenant.required",
                "Tenant identifier is required."));
        }

        if (licensePlanId.Value == Guid.Empty)
        {
            return Result.Failure(Error.Validation(
                "licensing.subscription.plan.required",
                "License plan identifier is required."));
        }

        if (trialDays < 1)
        {
            return Result.Failure(Error.Validation(
                "licensing.subscription.trial.invalid",
                "Trial days must be at least 1."));
        }

        DateTime utcNow = DateTime.UtcNow;

        TenantSubscription subscription = new(
            TenantSubscriptionId.New(),
            tenantId,
            licensePlanId,
            SubscriptionStatus.Trialing,
            utcNow,
            null,
            utcNow.AddDays(trialDays));

        return Result.Success(subscription);
    }

    /// <summary>
    /// Creates a new active paid tenant subscription.
    /// </summary>
    public static Result<TenantSubscription> CreateActive(
        TenantId tenantId,
        LicensePlanId licensePlanId,
        DateTime startsAtUtc)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Result.Failure<TenantSubscription>(Error.Validation(
                "licensing.subscription.tenant.required",
                "Tenant identifier is required."));
        }

        if (licensePlanId.Value == Guid.Empty)
        {
            return Result.Failure<TenantSubscription>(Error.Validation(
                "licensing.subscription.plan.required",
                "License plan identifier is required."));
        }

        TenantSubscription subscription = new(
            TenantSubscriptionId.New(),
            tenantId,
            licensePlanId,
            SubscriptionStatus.Active,
            startsAtUtc,
            null,
            null);

        return Result.Success(subscription);
    }

    /// <summary>
    /// Activates the subscription.
    /// </summary>
    public void Activate()
    {
        Status = SubscriptionStatus.Active;
        TrialEndsAtUtc = null;
    }

    /// <summary>
    /// Changes the plan.
    /// </summary>
    public void ChangePlan(LicensePlanId licensePlanId)
    {
        LicensePlanId = licensePlanId;
    }

    /// <summary>
    /// Marks the subscription as active for the provided billing period.
    /// </summary>
    public void SetActivePeriod(DateTime startsAtUtc, DateTime? endsAtUtc)
    {
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Status = SubscriptionStatus.Active;
        TrialEndsAtUtc = null;
    }

    /// <summary>
    /// Renews the subscription for a new billing period and keeps it active.
    /// </summary>
    public void Renew(DateTime startsAtUtc, DateTime? endsAtUtc)
    {
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Status = SubscriptionStatus.Active;
        TrialEndsAtUtc = null;
    }

    /// <summary>
    /// Cancels the subscription.
    /// </summary>
    public void Cancel(DateTime endsAtUtc)
    {
        Status = SubscriptionStatus.Cancelled;
        EndsAtUtc = endsAtUtc;
    }
}