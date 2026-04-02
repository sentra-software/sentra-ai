using Xunit;

namespace Sentra.IntegrationTests.Common;

/// <summary>
/// Defines a shared test collection for integration test fixtures.
/// </summary>
[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<TestServiceScopeFactory>
{
    /// <summary>
    /// The collection name.
    /// </summary>
    public const string Name = "IntegrationTests";
}