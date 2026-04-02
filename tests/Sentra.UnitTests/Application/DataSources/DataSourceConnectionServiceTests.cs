using Moq;
using Xunit;

using Sentra.Application.Abstractions.Connectors;
using Sentra.Application.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Contains tests for <see cref="DataSourceConnectionService"/>.
/// </summary>
public sealed class DataSourceConnectionServiceTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenConnectorRegistryIsNull()
    {
        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => new DataSourceConnectionService(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\r\n")]
    [InlineData("\t")]
    public async Task TestConnectionAsync_ShouldFail_WhenConnectionStringIsNullOrWhitespace(string? connectionString)
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        DataSourceConnectionService service = new(connectorRegistryMock.Object);

        // Act
        Result<ConnectionTestResult> result = await service.TestConnectionAsync(
            DataSourceType.PostgreSql,
            connectionString!,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("datasources.connection_string.required", result.Error.Code);

        connectorRegistryMock.Verify(
            registry => registry.GetConnector(It.IsAny<ConnectorType>()),
            Times.Never);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldFail_WhenDataSourceTypeIsInvalidEnumValue()
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        DataSourceConnectionService service = new(connectorRegistryMock.Object);

        DataSourceType invalidType = (DataSourceType)999;

        // Act
        Result<ConnectionTestResult> result = await service.TestConnectionAsync(
            invalidType,
            "Host=localhost;Database=sentra;",
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("datasources.type.invalid", result.Error.Code);

        connectorRegistryMock.Verify(
            registry => registry.GetConnector(It.IsAny<ConnectorType>()),
            Times.Never);
    }

    [Theory]
    [InlineData(DataSourceType.PostgreSql, ConnectorType.PostgreSql)]
    [InlineData(DataSourceType.SqlServer, ConnectorType.SqlServer)]
    [InlineData(DataSourceType.MySql, ConnectorType.MySql)]
    [InlineData(DataSourceType.Sqlite, ConnectorType.Sqlite)]
    public async Task TestConnectionAsync_ShouldResolveExpectedConnectorType(
        DataSourceType dataSourceType,
        ConnectorType expectedConnectorType)
    {
        // Arrange
        Mock<IDataConnector> connectorMock = new();
        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(expectedConnectorType);

        ConnectionTestResult expectedConnectionTestResult =
            ConnectionTestResult.Success("Connection succeeded.");

        connectorMock
            .Setup(connector => connector.TestConnectionAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedConnectionTestResult);

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(expectedConnectorType))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceConnectionService service = new(connectorRegistryMock.Object);

        // Act
        Result<ConnectionTestResult> result = await service.TestConnectionAsync(
            dataSourceType,
            "Host=localhost;Database=sentra;",
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(expectedConnectionTestResult, result.Value);

        connectorRegistryMock.Verify(
            registry => registry.GetConnector(expectedConnectorType),
            Times.Once);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldFail_WhenConnectorRegistryReturnsFailure()
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Failure<IDataConnector>(
                Error.Failure("connectors.not_registered", "Connector is not registered.")));

        DataSourceConnectionService service = new(connectorRegistryMock.Object);

        // Act
        Result<ConnectionTestResult> result = await service.TestConnectionAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("connectors.not_registered", result.Error.Code);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldTrimConnectionString_BeforePassingToConnector()
    {
        // Arrange
        const string rawConnectionString = "   Host=localhost;Database=sentra;   ";
        const string trimmedConnectionString = "Host=localhost;Database=sentra;";

        Mock<IDataConnector> connectorMock = new();
        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        connectorMock
            .Setup(connector => connector.TestConnectionAsync(
                trimmedConnectionString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConnectionTestResult.Success("Connection succeeded."));

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceConnectionService service = new(connectorRegistryMock.Object);

        // Act
        Result<ConnectionTestResult> result = await service.TestConnectionAsync(
            DataSourceType.PostgreSql,
            rawConnectionString,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        connectorMock.Verify(
            connector => connector.TestConnectionAsync(
                trimmedConnectionString,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldPassCancellationToken_ToConnector()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        Mock<IDataConnector> connectorMock = new();
        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        connectorMock
            .Setup(connector => connector.TestConnectionAsync(
                It.IsAny<string>(),
                cancellationToken))
            .ReturnsAsync(ConnectionTestResult.Success("Connection succeeded."));

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceConnectionService service = new(connectorRegistryMock.Object);

        // Act
        Result<ConnectionTestResult> result = await service.TestConnectionAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        connectorMock.Verify(
            connector => connector.TestConnectionAsync(
                It.IsAny<string>(),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldWrapSuccessfulConnectionTestResult()
    {
        // Arrange
        ConnectionTestResult expectedConnectionTestResult =
            ConnectionTestResult.Success("Database is reachable.");

        Mock<IDataConnector> connectorMock = new();
        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        connectorMock
            .Setup(connector => connector.TestConnectionAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedConnectionTestResult);

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceConnectionService service = new(connectorRegistryMock.Object);

        // Act
        Result<ConnectionTestResult> result = await service.TestConnectionAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(expectedConnectionTestResult, result.Value);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldReturnFailureResultFromConnector_AsSuccessfulServiceResultPayload()
    {
        // Arrange
        ConnectionTestResult failedConnectionTestResult =
            ConnectionTestResult.Failure("Could not connect to database.");

        Mock<IDataConnector> connectorMock = new();
        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        connectorMock
            .Setup(connector => connector.TestConnectionAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedConnectionTestResult);

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceConnectionService service = new(connectorRegistryMock.Object);

        // Act
        Result<ConnectionTestResult> result = await service.TestConnectionAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(failedConnectionTestResult, result.Value);
        Assert.False(result.Value!.IsSuccess);
    }
}