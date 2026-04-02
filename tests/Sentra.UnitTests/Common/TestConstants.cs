namespace Sentra.UnitTests.Common;

/// <summary>
/// Provides shared constants for unit tests.
/// </summary>
public static class TestConstants
{
    public const string ValidJwtIssuer = "sentra-test-issuer";
    public const string ValidJwtAudience = "sentra-test-audience";
    public const string ValidJwtSigningKey = "super-secure-signing-key-for-tests-123456789";
    public const string ValidConnectionString = "Host=localhost;Port=5432;Database=sentra_test;Username=test;Password=test123!";
}