using Xunit;

namespace Sentra.IntegrationTests.Common;

/// <summary>
/// Contains basic smoke tests for the integration test project.
/// </summary>
public sealed class SmokeTests
{
    [Fact]
    public void IntegrationTestProject_ShouldRun()
    {
        Assert.True(true);
    }
}