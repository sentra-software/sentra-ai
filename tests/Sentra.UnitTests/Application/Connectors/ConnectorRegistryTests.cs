using Moq;
using Xunit;

using Sentra.Application.Connectors;
using Sentra.Connectors.Abstractions.Connectors;

namespace Sentra.UnitTests.Application.Connectors;

/// <summary>
/// Contains tests for <see cref="ConnectorRegistry"/>.
/// </summary>
public sealed class ConnectorRegistryTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenConnectorsIsNull()
    {
        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => new ConnectorRegistry(null!));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDuplicateConnectorTypesAreRegistered()
    {
        // Arrange
        Mock<IDataConnector> first = CreateConnector(ConnectorType.PostgreSql);
        Mock<IDataConnector> second = CreateConnector(ConnectorType.PostgreSql);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() =>
            new ConnectorRegistry([first.Object, second.Object]));
    }

    [Fact]
    public void RegisteredTypes_ShouldContainAllUniqueRegisteredConnectorTypes()
    {
        // Arrange
        Mock<IDataConnector> postgres = CreateConnector(ConnectorType.PostgreSql);
        Mock<IDataConnector> sqlServer = CreateConnector(ConnectorType.SqlServer);
        ConnectorRegistry registry = new([postgres.Object, sqlServer.Object]);

        // Act
        IReadOnlyCollection<ConnectorType> registeredTypes = registry.RegisteredTypes;

        // Assert
        Assert.Equal(2, registeredTypes.Count);
        Assert.Contains(ConnectorType.PostgreSql, registeredTypes);
        Assert.Contains(ConnectorType.SqlServer, registeredTypes);
    }

    [Fact]
    public void GetConnector_ShouldReturnSuccess_WhenConnectorIsRegistered()
    {
        // Arrange
        Mock<IDataConnector> postgres = CreateConnector(ConnectorType.PostgreSql);
        ConnectorRegistry registry = new([postgres.Object]);

        // Act
        var result = registry.GetConnector(ConnectorType.PostgreSql);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Same(postgres.Object, result.Value);
    }

    [Fact]
    public void GetConnector_ShouldReturnFailure_WhenConnectorIsNotRegistered()
    {
        // Arrange
        Mock<IDataConnector> postgres = CreateConnector(ConnectorType.PostgreSql);
        ConnectorRegistry registry = new([postgres.Object]);

        // Act
        var result = registry.GetConnector(ConnectorType.MySql);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Equal("connectors.not_registered", result.Error.Code);
    }

    [Fact]
    public void GetConnector_ShouldReturnFailureMessageContainingRequestedType_WhenConnectorIsNotRegistered()
    {
        // Arrange
        ConnectorRegistry registry = new([]);

        // Act
        var result = registry.GetConnector(ConnectorType.Sqlite);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("Sqlite", result.Error.Message);
    }

    private static Mock<IDataConnector> CreateConnector(ConnectorType connectorType)
    {
        Mock<IDataConnector> connector = new();
        connector.SetupGet(x => x.Type).Returns(connectorType);
        connector.SetupGet(x => x.Capabilities).Returns(Array.Empty<ConnectorCapability>());

        return connector;
    }
}