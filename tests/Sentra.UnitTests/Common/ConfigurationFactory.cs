using Microsoft.Extensions.Configuration;
using Sentra.Infrastructure.Billing.Stripe;
using Sentra.Security.Jwt;

namespace Sentra.UnitTests.Common;

/// <summary>
/// Creates reusable configuration objects for test scenarios.
/// </summary>
public static class ConfigurationFactory
{
    /// <summary>
    /// Builds a valid JWT configuration section.
    /// </summary>
    public static IConfiguration CreateJwtConfiguration(
        string? issuer = TestConstants.ValidJwtIssuer,
        string? audience = TestConstants.ValidJwtAudience,
        string? signingKey = TestConstants.ValidJwtSigningKey,
        string? expirationInMinutes = "60")
    {
        Dictionary<string, string?> values = new()
        {
            [$"{JwtOptions.SectionName}:Issuer"] = issuer,
            [$"{JwtOptions.SectionName}:Audience"] = audience,
            [$"{JwtOptions.SectionName}:SigningKey"] = signingKey,
            [$"{JwtOptions.SectionName}:ExpirationInMinutes"] = expirationInMinutes
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    /// <summary>
    /// Builds a valid infrastructure configuration section.
    /// </summary>
    public static IConfiguration CreateInfrastructureConfiguration(
        string connectionString = TestConstants.ValidConnectionString,
        string stripeSecretKey = "sk_test_default",
        string stripeWebhookSecret = "whsec_default",
        string starterMonthlyPriceId = "price_default")
    {
        Dictionary<string, string?> values = new()
        {
            ["ConnectionStrings:SentraPlatform"] = connectionString,
            [$"{StripeBillingOptions.SectionName}:SecretKey"] = stripeSecretKey,
            [$"{StripeBillingOptions.SectionName}:WebhookSecret"] = stripeWebhookSecret,
            [$"{StripeBillingOptions.SectionName}:StarterMonthlyPriceId"] = starterMonthlyPriceId
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}