using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Sentra.Domain.Licensing;
using Sentra.Domain.Tenants;
using Sentra.Infrastructure.Billing.Stripe;
using Sentra.Infrastructure.Persistence;

using Stripe.Checkout;
using Xunit;
using StripeSubscription = Stripe.Subscription;

namespace Sentra.UnitTests.Infrastructure.Billing;

/// <summary>
/// Contains tests for <see cref="StripeWebhookService"/>.
/// </summary>
public sealed class StripeWebhookServiceTests
{
    [Fact]
    public async Task HandleCheckoutCompletedAsync_ShouldThrow_WhenTenantIdIsMissing()
    {
        // Arrange
        SentraPlatformDbContext dbContext = CreateDbContext();
        StripeWebhookService service = CreateService(dbContext);

        Session session = new()
        {
            Metadata = new Dictionary<string, string>
            {
                ["plan_code"] = "STARTER",
                ["billing_interval"] = "MONTHLY"
            }
        };

        // Act
        Func<Task> act = () => service.HandleCheckoutCompletedAsync(session);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleCheckoutCompletedAsync_ShouldThrow_WhenPlanCodeIsMissing()
    {
        // Arrange
        SentraPlatformDbContext dbContext = CreateDbContext();
        StripeWebhookService service = CreateService(dbContext);

        Session session = new()
        {
            Metadata = new Dictionary<string, string>
            {
                ["tenant_id"] = Guid.NewGuid().ToString(),
                ["billing_interval"] = "MONTHLY"
            }
        };

        // Act
        Func<Task> act = () => service.HandleCheckoutCompletedAsync(session);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleCheckoutCompletedAsync_ShouldThrow_WhenBillingIntervalIsMissing()
    {
        // Arrange
        SentraPlatformDbContext dbContext = CreateDbContext();
        StripeWebhookService service = CreateService(dbContext);

        Session session = new()
        {
            Metadata = new Dictionary<string, string>
            {
                ["tenant_id"] = Guid.NewGuid().ToString(),
                ["plan_code"] = "STARTER"
            }
        };

        // Act
        Func<Task> act = () => service.HandleCheckoutCompletedAsync(session);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleCheckoutCompletedAsync_ShouldThrow_WhenNoActivePlanExists()
    {
        // Arrange
        SentraPlatformDbContext dbContext = CreateDbContext();
        StripeWebhookService service = CreateService(dbContext);

        Session session = new()
        {
            Metadata = new Dictionary<string, string>
            {
                ["tenant_id"] = Guid.NewGuid().ToString(),
                ["plan_code"] = "STARTER",
                ["billing_interval"] = "MONTHLY"
            }
        };

        // Act
        Func<Task> act = () => service.HandleCheckoutCompletedAsync(session);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleSubscriptionUpdatedAsync_ShouldNotThrow_WhenTenantIdIsMissing()
    {
        // Arrange
        SentraPlatformDbContext dbContext = CreateDbContext();
        StripeWebhookService service = CreateService(dbContext);

        StripeSubscription subscription = new()
        {
            Id = "sub_123",
            Status = "active",
            Metadata = new Dictionary<string, string>()
        };

        // Act
        Func<Task> act = () => service.HandleSubscriptionUpdatedAsync(subscription);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleSubscriptionDeletedAsync_ShouldNotThrow_WhenTenantIdIsMissing()
    {
        // Arrange
        SentraPlatformDbContext dbContext = CreateDbContext();
        StripeWebhookService service = CreateService(dbContext);

        StripeSubscription subscription = new()
        {
            Id = "sub_123",
            Metadata = new Dictionary<string, string>()
        };

        // Act
        Func<Task> act = () => service.HandleSubscriptionDeletedAsync(subscription);

        // Assert
        await act.Should().NotThrowAsync();
    }

    private static StripeWebhookService CreateService(SentraPlatformDbContext dbContext)
    {
        return new StripeWebhookService(
            dbContext,
            NullLogger<StripeWebhookService>.Instance);
    }

    private static SentraPlatformDbContext CreateDbContext()
    {
        DbContextOptions<SentraPlatformDbContext> options =
            new DbContextOptionsBuilder<SentraPlatformDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new SentraPlatformDbContext(options);
    }
}