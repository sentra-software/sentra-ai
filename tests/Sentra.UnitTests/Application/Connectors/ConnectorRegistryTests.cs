using FluentAssertions;
using Sentra.Application.Connectors;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Application.Connectors;

/// <summary>
/// Contains unit tests for <see cref="ConnectorRegistry"/>.
/// </summary>
public sealed class ConnectorRegistryTests
{
    /// <summary>
    /// Verifies that the registry returns a registered connector.
    /// </summary>
    [Fact]
    public void GetConnector_Should_Return_Success_When_Connector_Is_Registered()
    {
        TestDataConnector? postgresConnector = new TestDataConnector(ConnectorType.PostgreSql);
        ConnectorRegistry? registry = new ConnectorRegistry([postgresConnector]);

        Result<IDataConnector>? result = registry.GetConnector(ConnectorType.PostgreSql);

        result.IsSuccess.Should().BeTrue();
        result.ValueOrThrow().Should().BeSameAs(postgresConnector);
    }

    /// <summary>
    /// Verifies that the registry returns a failure when a connector is not registered.
    /// </summary>
    [Fact]
    public void GetConnector_Should_Return_Failure_When_Connector_Is_Not_Registered()
    {
        ConnectorRegistry? registry = new ConnectorRegistry(
        [
            new TestDataConnector(ConnectorType.PostgreSql)
        ]);

        Result<IDataConnector>? result = registry.GetConnector(ConnectorType.MySql);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("connectors.not_registered");
    }

    /// <summary>
    /// Verifies that all registered connector types are exposed.
    /// </summary>
    [Fact]
    public void RegisteredTypes_Should_Return_All_Registered_Connector_Types()
    {
        ConnectorRegistry? registry = new ConnectorRegistry(
        [
            new TestDataConnector(ConnectorType.PostgreSql),
            new TestDataConnector(ConnectorType.SqlServer)
        ]);

        registry.RegisteredTypes.Should().HaveCount(2);
        registry.RegisteredTypes.Should().Contain(ConnectorType.PostgreSql);
        registry.RegisteredTypes.Should().Contain(ConnectorType.SqlServer);
    }

    /// <summary>
    /// Verifies that duplicate connector types are rejected.
    /// </summary>
    [Fact]
    public void Constructor_Should_Throw_When_Duplicate_Connector_Types_Are_Registered()
    {
        Func<ConnectorRegistry>? action = () => new ConnectorRegistry(
        [
            new TestDataConnector(ConnectorType.PostgreSql),
            new TestDataConnector(ConnectorType.PostgreSql)
        ]);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Multiple connectors are registered for type 'PostgreSql'.");
    }

    /// <summary>
    /// Verifies that passing a null connector collection throws an exception.
    /// </summary>
    [Fact]
    public void Constructor_Should_Throw_When_Connectors_Are_Null()
    {
        Func<ConnectorRegistry>? action = () => new ConnectorRegistry(null!);

        action.Should().Throw<ArgumentNullException>();
    }
}