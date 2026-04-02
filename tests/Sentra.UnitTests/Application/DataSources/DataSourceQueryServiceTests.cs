using Moq;
using Xunit;

using Sentra.Application.Abstractions.Connectors;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Application.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Contains tests for <see cref="DataSourceQueryService"/>.
/// </summary>
public sealed class DataSourceQueryServiceTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenConnectorRegistryIsNull()
    {
        // Arrange
        Mock<IQuerySafetyValidator> querySafetyValidatorMock = new();

        // Act + Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DataSourceQueryService(null!, querySafetyValidatorMock.Object));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQuerySafetyValidatorIsNull()
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();

        // Act + Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DataSourceQueryService(connectorRegistryMock.Object, null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\r\n")]
    [InlineData("\t")]
    public async Task ExecuteQueryAsync_ShouldFail_WhenConnectionStringIsNullOrWhitespace(string? connectionString)
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        Mock<IQuerySafetyValidator> querySafetyValidatorMock = new();

        DataSourceQueryService service = new(
            connectorRegistryMock.Object,
            querySafetyValidatorMock.Object);

        // Act
        Result<QueryExecutionResult> result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            connectionString!,
            "SELECT 1",
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("datasources.connection_string.required", result.Error.Code);

        querySafetyValidatorMock.Verify(
            validator => validator.Validate(It.IsAny<string>()),
            Times.Never);

        connectorRegistryMock.Verify(
            registry => registry.GetConnector(It.IsAny<ConnectorType>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldFail_WhenQuerySafetyValidationFails()
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        Mock<IQuerySafetyValidator> querySafetyValidatorMock = new();

        querySafetyValidatorMock
            .Setup(validator => validator.Validate("DELETE FROM Users"))
            .Returns(Result.Failure(
                Error.Validation(
                    "datasources.query.only_select_allowed",
                    "Only SELECT queries are allowed.")));

        DataSourceQueryService service = new(
            connectorRegistryMock.Object,
            querySafetyValidatorMock.Object);

        // Act
        Result<QueryExecutionResult> result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            "DELETE FROM Users",
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("datasources.query.only_select_allowed", result.Error.Code);

        connectorRegistryMock.Verify(
            registry => registry.GetConnector(It.IsAny<ConnectorType>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldPassOriginalQuery_ToQuerySafetyValidator()
    {
        // Arrange
        const string rawQuery = "   SELECT * FROM Users   ";

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        Mock<IQuerySafetyValidator> querySafetyValidatorMock = new();
        Mock<IDataConnector> connectorMock = new();

        querySafetyValidatorMock
            .Setup(validator => validator.Validate(rawQuery))
            .Returns(Result.Success());

        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        QueryExecutionResult executionResult = CreateQueryExecutionResult();

        connectorMock
            .Setup(connector => connector.ExecuteQueryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(executionResult);

        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceQueryService service = new(
            connectorRegistryMock.Object,
            querySafetyValidatorMock.Object);

        // Act
        Result<QueryExecutionResult> result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            rawQuery,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        querySafetyValidatorMock.Verify(
            validator => validator.Validate(rawQuery),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldFail_WhenDataSourceTypeIsInvalidEnumValue()
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        Mock<IQuerySafetyValidator> querySafetyValidatorMock = new();

        querySafetyValidatorMock
            .Setup(validator => validator.Validate("SELECT 1"))
            .Returns(Result.Success());

        DataSourceQueryService service = new(
            connectorRegistryMock.Object,
            querySafetyValidatorMock.Object);

        DataSourceType invalidType = (DataSourceType)999;

        // Act
        Result<QueryExecutionResult> result = await service.ExecuteQueryAsync(
            invalidType,
            "Host=localhost;Database=sentra;",
            "SELECT 1",
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
    public async Task ExecuteQueryAsync_ShouldResolveExpectedConnectorType(
        DataSourceType dataSourceType,
        ConnectorType expectedConnectorType)
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        Mock<IQuerySafetyValidator> querySafetyValidatorMock = new();
        Mock<IDataConnector> connectorMock = new();

        querySafetyValidatorMock
            .Setup(validator => validator.Validate("SELECT 1"))
            .Returns(Result.Success());

        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(expectedConnectorType);

        QueryExecutionResult executionResult = CreateQueryExecutionResult();

        connectorMock
            .Setup(connector => connector.ExecuteQueryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(executionResult);

        connectorRegistryMock
            .Setup(registry => registry.GetConnector(expectedConnectorType))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceQueryService service = new(
            connectorRegistryMock.Object,
            querySafetyValidatorMock.Object);

        // Act
        Result<QueryExecutionResult> result = await service.ExecuteQueryAsync(
            dataSourceType,
            "Host=localhost;Database=sentra;",
            "SELECT 1",
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        connectorRegistryMock.Verify(
            registry => registry.GetConnector(expectedConnectorType),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldFail_WhenConnectorRegistryReturnsFailure()
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        Mock<IQuerySafetyValidator> querySafetyValidatorMock = new();

        querySafetyValidatorMock
            .Setup(validator => validator.Validate("SELECT 1"))
            .Returns(Result.Success());

        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Failure<IDataConnector>(
                Error.Failure(
                    "connectors.not_registered",
                    "Connector is not registered.")));

        DataSourceQueryService service = new(
            connectorRegistryMock.Object,
            querySafetyValidatorMock.Object);

        // Act
        Result<QueryExecutionResult> result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            "SELECT 1",
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("connectors.not_registered", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldTrimConnectionString_AndQuery_BeforePassingToConnector()
    {
        // Arrange
        const string rawConnectionString = "   Host=localhost;Database=sentra;   ";
        const string trimmedConnectionString = "Host=localhost;Database=sentra;";
        const string rawQuery = "   SELECT * FROM Users   ";
        const string trimmedQuery = "SELECT * FROM Users";

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        Mock<IQuerySafetyValidator> querySafetyValidatorMock = new();
        Mock<IDataConnector> connectorMock = new();

        querySafetyValidatorMock
            .Setup(validator => validator.Validate(rawQuery))
            .Returns(Result.Success());

        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        QueryExecutionResult executionResult = CreateQueryExecutionResult();

        connectorMock
            .Setup(connector => connector.ExecuteQueryAsync(
                trimmedConnectionString,
                trimmedQuery,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(executionResult);

        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceQueryService service = new(
            connectorRegistryMock.Object,
            querySafetyValidatorMock.Object);

        // Act
        Result<QueryExecutionResult> result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            rawConnectionString,
            rawQuery,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        connectorMock.Verify(
            connector => connector.ExecuteQueryAsync(
                trimmedConnectionString,
                trimmedQuery,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldPassCancellationToken_ToConnector()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        Mock<IConnectorRegistry> connectorRegistryMock = new();
        Mock<IQuerySafetyValidator> querySafetyValidatorMock = new();
        Mock<IDataConnector> connectorMock = new();

        querySafetyValidatorMock
            .Setup(validator => validator.Validate("SELECT 1"))
            .Returns(Result.Success());

        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        QueryExecutionResult executionResult = CreateQueryExecutionResult();

        connectorMock
            .Setup(connector => connector.ExecuteQueryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                cancellationToken))
            .ReturnsAsync(executionResult);

        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceQueryService service = new(
            connectorRegistryMock.Object,
            querySafetyValidatorMock.Object);

        // Act
        Result<QueryExecutionResult> result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            "SELECT 1",
            cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        connectorMock.Verify(
            connector => connector.ExecuteQueryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldWrapConnectorExecutionResult_InSuccessfulResult()
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        Mock<IQuerySafetyValidator> querySafetyValidatorMock = new();
        Mock<IDataConnector> connectorMock = new();

        querySafetyValidatorMock
            .Setup(validator => validator.Validate("SELECT 1"))
            .Returns(Result.Success());

        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        QueryExecutionResult expectedExecutionResult = CreateQueryExecutionResult();

        connectorMock
            .Setup(connector => connector.ExecuteQueryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedExecutionResult);

        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceQueryService service = new(
            connectorRegistryMock.Object,
            querySafetyValidatorMock.Object);

        // Act
        Result<QueryExecutionResult> result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            "SELECT 1",
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(expectedExecutionResult, result.Value);
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldReturnSuccessfulServiceResult_EvenWhenConnectorReturnsEmptyRows()
    {
        // Arrange
        Mock<IConnectorRegistry> connectorRegistryMock = new();
        Mock<IQuerySafetyValidator> querySafetyValidatorMock = new();
        Mock<IDataConnector> connectorMock = new();

        querySafetyValidatorMock
            .Setup(validator => validator.Validate("SELECT 1"))
            .Returns(Result.Success());

        connectorMock
            .SetupGet(connector => connector.Type)
            .Returns(ConnectorType.PostgreSql);

        QueryExecutionResult emptyExecutionResult = CreateQueryExecutionResult(
            columns: ["Id", "Name"],
            rows: []);

        connectorMock
            .Setup(connector => connector.ExecuteQueryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyExecutionResult);

        connectorRegistryMock
            .Setup(registry => registry.GetConnector(ConnectorType.PostgreSql))
            .Returns(Result.Success(connectorMock.Object));

        DataSourceQueryService service = new(
            connectorRegistryMock.Object,
            querySafetyValidatorMock.Object);

        // Act
        Result<QueryExecutionResult> result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            "SELECT 1",
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(emptyExecutionResult, result.Value);
    }

    private static QueryExecutionResult CreateQueryExecutionResult(
        IReadOnlyList<string>? columns = null,
        IReadOnlyList<IReadOnlyDictionary<string, object?>>? rows = null)
    {
        columns ??= ["Id", "Name"];
        rows ??=
        [
            new Dictionary<string, object?>
            {
                ["Id"] = 1,
                ["Name"] = "Josey"
            }
        ];

        return new QueryExecutionResult(columns, rows, 1);
    }
}