using FluentAssertions;
using Sentra.Connectors.Abstractions.Connectors;

namespace Sentra.UnitTests.Connectors.Abstractions.Connectors;

/// <summary>
/// Contains unit tests for <see cref="ConnectionTestResult"/>.
/// </summary>
public sealed class ConnectionTestResultTests
{
    /// <summary>
    /// Verifies that a successful connection result is created correctly.
    /// </summary>
    [Fact]
    public void Success_Should_Create_Successful_Result()
    {
        ConnectionTestResult? result = ConnectionTestResult.Success("Connection succeeded.");

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Connection succeeded.");
    }

    /// <summary>
    /// Verifies that a failed connection result is created correctly.
    /// </summary>
    [Fact]
    public void Failure_Should_Create_Failed_Result()
    {
        ConnectionTestResult? result = ConnectionTestResult.Failure("Connection failed.");

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Connection failed.");
    }
}