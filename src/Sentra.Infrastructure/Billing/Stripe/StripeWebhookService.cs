using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sentra.Application.Abstractions.Billing;
using Sentra.Domain.Licensing;
using Sentra.Domain.Tenants;
using Sentra.Infrastructure.Persistence;
using Sentra.SharedKernel.Results;
using Stripe.Checkout;
using StripeSubscription = Stripe.Subscription;

namespace Sentra.Infrastructure.Billing.Stripe;

/// <summary>
/// Synchronizes Stripe webhook events into Sentra billing and licensing state.
/// </summary>
public sealed class StripeWebhookService : IStripeWebhookService
{
    private readonly SentraPlatformDbContext _dbContext;
    private readonly ILogger<StripeWebhookService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StripeWebhookService"/> class.
    /// </summary>
    public StripeWebhookService(
        SentraPlatformDbContext dbContext,
        ILogger<StripeWebhookService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleCheckoutCompletedAsync(
        Session session,
        CancellationToken cancellationToken = default)
    {
        string? tenantIdValue = session.Metadata?.GetValueOrDefault("tenant_id");
        string? planCode = session.Metadata?.GetValueOrDefault("plan_code");

        if (!Guid.TryParse(tenantIdValue, out Guid tenantId))
        {
            throw new InvalidOperationException(
                "Stripe checkout session did not contain a valid tenant_id.");
        }

        if (string.IsNullOrWhiteSpace(planCode))
        {
            throw new InvalidOperationException(
                "Stripe checkout session did not contain a valid plan_code.");
        }

        string normalizedPlanCode = planCode.Trim().ToUpperInvariant();

        LicensePlan? plan = await _dbContext.LicensePlans
            .FirstOrDefaultAsync(
                x => x.Code == normalizedPlanCode && x.IsActive,
                cancellationToken);

        if (plan is null)
        {
            throw new InvalidOperationException(
                $"No active Sentra license plan was found for code '{normalizedPlanCode}'.");
        }

        TenantId typedTenantId = new(tenantId);

        TenantSubscription? existingSubscription = await _dbContext.TenantSubscriptions
            .FirstOrDefaultAsync(
                x => x.TenantId == typedTenantId,
                cancellationToken);

        if (existingSubscription is null)
        {
            Result<TenantSubscription> createResult = TenantSubscription.CreateActive(
                typedTenantId,
                plan.Id,
                DateTime.UtcNow);

            if (createResult.IsFailure)
            {
                throw new InvalidOperationException(createResult.Error.Message);
            }

            _dbContext.TenantSubscriptions.Add(createResult.ValueOrThrow());

            _logger.LogInformation(
                "Created Sentra tenant subscription for tenant {TenantId} with plan {PlanCode}.",
                tenantId,
                normalizedPlanCode);
        }
        else
        {
            existingSubscription.ChangePlan(plan.Id);
            existingSubscription.Activate();

            _logger.LogInformation(
                "Updated Sentra tenant subscription for tenant {TenantId} to plan {PlanCode}.",
                tenantId,
                normalizedPlanCode);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task HandleSubscriptionUpdatedAsync(
        StripeSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        string? tenantIdValue = subscription.Metadata?.GetValueOrDefault("tenant_id");
        string? planCode = subscription.Metadata?.GetValueOrDefault("plan_code");

        if (!Guid.TryParse(tenantIdValue, out Guid tenantId))
        {
            _logger.LogWarning(
                "Stripe subscription update ignored because tenant_id metadata was missing or invalid. SubscriptionId={SubscriptionId}",
                subscription.Id);

            return;
        }

        TenantId typedTenantId = new(tenantId);

        TenantSubscription? existingSubscription = await _dbContext.TenantSubscriptions
            .FirstOrDefaultAsync(
                x => x.TenantId == typedTenantId,
                cancellationToken);

        if (existingSubscription is null)
        {
            _logger.LogWarning(
                "Stripe subscription update ignored because no Sentra tenant subscription exists yet. TenantId={TenantId}, SubscriptionId={SubscriptionId}",
                tenantId,
                subscription.Id);

            return;
        }

        if (!string.IsNullOrWhiteSpace(planCode))
        {
            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();

            LicensePlan? plan = await _dbContext.LicensePlans
                .FirstOrDefaultAsync(
                    x => x.Code == normalizedPlanCode && x.IsActive,
                    cancellationToken);

            if (plan is not null)
            {
                existingSubscription.ChangePlan(plan.Id);
            }
        }

        switch (subscription.Status?.Trim().ToLowerInvariant())
        {
            case "trialing":
            case "active":
                existingSubscription.Activate();
                break;

            case "past_due":
            case "unpaid":
                // Minimal working version:
                // keep the subscription active in the entity shape you currently have.
                // You can extend the domain later with a MarkPastDue() method.
                break;

            case "canceled":
                existingSubscription.Cancel(DateTime.UtcNow);
                break;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Processed Stripe subscription update for tenant {TenantId}. StripeStatus={StripeStatus}",
            tenantId,
            subscription.Status);
    }

    /// <inheritdoc />
    public async Task HandleSubscriptionDeletedAsync(
        StripeSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        string? tenantIdValue = subscription.Metadata?.GetValueOrDefault("tenant_id");

        if (!Guid.TryParse(tenantIdValue, out Guid tenantId))
        {
            _logger.LogWarning(
                "Stripe subscription deletion ignored because tenant_id metadata was missing or invalid. SubscriptionId={SubscriptionId}",
                subscription.Id);

            return;
        }

        TenantId typedTenantId = new(tenantId);

        TenantSubscription? existingSubscription = await _dbContext.TenantSubscriptions
            .FirstOrDefaultAsync(
                x => x.TenantId == typedTenantId,
                cancellationToken);

        if (existingSubscription is null)
        {
            _logger.LogWarning(
                "Stripe subscription deletion ignored because no Sentra tenant subscription exists. TenantId={TenantId}, SubscriptionId={SubscriptionId}",
                tenantId,
                subscription.Id);

            return;
        }

        existingSubscription.Cancel(DateTime.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cancelled Sentra tenant subscription for tenant {TenantId}.",
            tenantId);
    }
}