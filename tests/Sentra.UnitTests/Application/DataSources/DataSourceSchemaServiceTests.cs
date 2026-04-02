using Moq;
using Xunit;

using Sentra.Application.Abstractions.Connectors;
using Sentra.Application.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Contains tests for <see cref="DataSourceSchemaService"/>.
/// </summary>
public sealed class DataSourceSchemaServiceTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenConnectorRegistryIsNull()
    {
        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => new DataSourceSchemaService(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\r\n")]
    [InlineData("\t")]
    public async Task ReadSchemaAsync_ShouldFail_WhenConnectionStringIsNullOrWhitespace(string? connectionString)
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        DataSourceSchemaService service = new(connectorRegistryMock.Object);

        // Act
        Result<IReadOnlyCollection<TableSchema>> result = await service.ReadSchemaAsync(
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
    public async Task ReadSchemaAsync_ShouldFail_WhenDataSourceTypeIsInvalidEnumValue()
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        DataSourceSchemaService service = new(connectorRegistryMock.Object);

        DataSourceType invalidType = (DataSourceType)999;

        // Act
        Result<IReadOnlyCollection<TableSchema>> result = await service.ReadSchemaAsync(
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
    public async Task ReadSchemaAsync_ShouldResolveExpectedConnectorType(
        DataSourceType dataSourceType,
        ConnectorType expectedConnectorType)
    {
        // Arrange
        Mock<IDataConnector> connectorMock = new();
        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(expectedConnectorType);

        IReadOnlyCollection<TableSchema> expectedSchema = CreateSchema();

        connectorMock
            .Setup(connector => connector.ReadSchemaAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSchema);

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(expectedConnectorType))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceSchemaService service = new(connectorRegistryMock.Object);

        // Act
        Result<IReadOnlyCollection<TableSchema>> result = await service.ReadSchemaAsync(
            dataSourceType,
            "Host=localhost;Database=sentra;",
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(expectedSchema, result.Value);

        connectorRegistryMock.Verify(
            registry => registry.GetConnector(expectedConnectorType),
            Times.Once);
    }

    [Fact]
    public async Task ReadSchemaAsync_ShouldFail_WhenConnectorRegistryReturnsFailure()
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Failure<IDataConnector>(
                Error.Failure(
                    "connectors.not_registered",
                    "Connector is not registered.")));

        DataSourceSchemaService service = new(connectorRegistryMock.Object);

        // Act
        Result<IReadOnlyCollection<TableSchema>> result = await service.ReadSchemaAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("connectors.not_registered", result.Error.Code);
    }

    [Fact]
    public async Task ReadSchemaAsync_ShouldTrimConnectionString_BeforePassingToConnector()
    {
        // Arrange
        const string rawConnectionString = "   Host=localhost;Database=sentra;   ";
        const string trimmedConnectionString = "Host=localhost;Database=sentra;";

        IReadOnlyCollection<TableSchema> expectedSchema = CreateSchema();

        Mock<IDataConnector> connectorMock = new();
        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        connectorMock
            .Setup(connector => connector.ReadSchemaAsync(
                trimmedConnectionString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSchema);

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceSchemaService service = new(connectorRegistryMock.Object);

        // Act
        Result<IReadOnlyCollection<TableSchema>> result = await service.ReadSchemaAsync(
            DataSourceType.PostgreSql,
            rawConnectionString,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        connectorMock.Verify(
            connector => connector.ReadSchemaAsync(
                trimmedConnectionString,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReadSchemaAsync_ShouldPassCancellationToken_ToConnector()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        IReadOnlyCollection<TableSchema> expectedSchema = CreateSchema();

        Mock<IDataConnector> connectorMock = new();
        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        connectorMock
            .Setup(connector => connector.ReadSchemaAsync(
                It.IsAny<string>(),
                cancellationToken))
            .ReturnsAsync(expectedSchema);

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceSchemaService service = new(connectorRegistryMock.Object);

        // Act
        Result<IReadOnlyCollection<TableSchema>> result = await service.ReadSchemaAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        connectorMock.Verify(
            connector => connector.ReadSchemaAsync(
                It.IsAny<string>(),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ReadSchemaAsync_ShouldWrapSchema_InSuccessfulResult()
    {
        // Arrange
        IReadOnlyCollection<TableSchema> expectedSchema = CreateSchema();

        Mock<IDataConnector> connectorMock = new();
        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        connectorMock
            .Setup(connector => connector.ReadSchemaAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSchema);

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceSchemaService service = new(connectorRegistryMock.Object);

        // Act
        Result<IReadOnlyCollection<TableSchema>> result = await service.ReadSchemaAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(expectedSchema, result.Value);
    }

    [Fact]
    public async Task ReadSchemaAsync_ShouldReturnSuccessfulResult_WhenSchemaIsEmpty()
    {
        // Arrange
        IReadOnlyCollection<TableSchema> emptySchema = Array.Empty<TableSchema>();

        Mock<IDataConnector> connectorMock = new();
        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        connectorMock
            .Setup(connector => connector.ReadSchemaAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptySchema);

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceSchemaService service = new(connectorRegistryMock.Object);

        // Act
        Result<IReadOnlyCollection<TableSchema>> result = await service.ReadSchemaAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(emptySchema, result.Value);
        Assert.Empty(result.Value!);
    }

    private static IReadOnlyCollection<TableSchema> CreateSchema()
    {
        return
        [
            new TableSchema(
                "public",
                "Users",
                [
                    new ColumnSchema("Id", "integer", false),
                    new ColumnSchema("Email", "text", false),
                    new ColumnSchema("DisplayName", "text", true)
                ])
        ];
    }
}