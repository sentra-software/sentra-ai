namespace Sentra.Domain.Licensing;

/// <summary>
/// Represents the lifecycle state of a tenant subscription.
/// </summary>
public enum SubscriptionStatus
{
    Trialing = 1,
    Active = 2,
    PastDue = 3,
    Cancelled = 4,
    Expired = 5
}