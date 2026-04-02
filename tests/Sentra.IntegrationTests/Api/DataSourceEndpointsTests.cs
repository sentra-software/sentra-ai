using System.Net;
using System.Net.Http.Json;
using Moq;
using Sentra.Api.Models;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Contracts.DataSources;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

using Xunit;

namespace Sentra.IntegrationTests.Api;

/// <summary>
/// Contains integration tests for data source endpoints.
/// </summary>
public sealed class DataSourceEndpointsTests
{
    [Fact]
    public async Task TestConnection_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        Mock<IDataSourceConnectionService> connectionServiceMock = new();

        TestDataSourceConnectionRequest request = new(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;");

        connectionServiceMock
            .Setup(service => service.TestConnectionAsync(
                request.DataSourceType,
                request.ConnectionString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ConnectionTestResult>(
                Error.Validation(
                    "datasources.connection_string.required",
                    "Connection string is required.")));

        await using CustomWebApplicationFactory factory = new(connectionServiceMock: connectionServiceMock);
        using HttpClient httpClient = factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "/api/data-sources/test-connection",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ApiErrorResponse? error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("datasources.connection_string.required", error.Code);
    }

    [Fact]
    public async Task TestConnection_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        Mock<IDataSourceConnectionService> connectionServiceMock = new();

        TestDataSourceConnectionRequest request = new(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;");

        connectionServiceMock
            .Setup(service => service.TestConnectionAsync(
                request.DataSourceType,
                request.ConnectionString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ConnectionTestResult.Success("Connection succeeded.")));

        await using CustomWebApplicationFactory factory = new(connectionServiceMock: connectionServiceMock);
        using HttpClient httpClient = factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "/api/data-sources/test-connection",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReadSchema_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        Mock<IDataSourceSchemaService> schemaServiceMock = new();

        ReadDataSourceSchemaRequest request = new(
            DataSourceType.Sqlite,
            "Data Source=:memory:");

        schemaServiceMock
            .Setup(service => service.ReadSchemaAsync(
                request.DataSourceType,
                request.ConnectionString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<IReadOnlyCollection<TableSchema>>(
                Error.Failure(
                    "connectors.not_registered",
                    "Connector is not registered.")));

        await using CustomWebApplicationFactory factory = new(schemaServiceMock: schemaServiceMock);
        using HttpClient httpClient = factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "/api/data-sources/read-schema",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ApiErrorResponse? error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("connectors.not_registered", error.Code);
    }

    [Fact]
    public async Task ReadSchema_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        Mock<IDataSourceSchemaService> schemaServiceMock = new();

        ReadDataSourceSchemaRequest request = new(
            DataSourceType.Sqlite,
            "Data Source=:memory:");

        IReadOnlyCollection<TableSchema> schema =
        [
            new TableSchema(
                "main",
                "Users",
                [
                    new ColumnSchema("Id", "INTEGER", false),
                    new ColumnSchema("Email", "TEXT", false)
                ])
        ];

        schemaServiceMock
            .Setup(service => service.ReadSchemaAsync(
                request.DataSourceType,
                request.ConnectionString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(schema));

        await using CustomWebApplicationFactory factory = new(schemaServiceMock: schemaServiceMock);
        using HttpClient httpClient = factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "/api/data-sources/read-schema",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteQuery_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        Mock<IDataSourceQueryService> queryServiceMock = new();

        ExecuteDataSourceQueryRequest request = new(
            DataSourceType.Sqlite,
            "Data Source=:memory:",
            "DELETE FROM Users");

        queryServiceMock
            .Setup(service => service.ExecuteQueryAsync(
                request.DataSourceType,
                request.ConnectionString,
                request.Query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<QueryExecutionResult>(
                Error.Validation(
                    "datasources.query.only_select_allowed",
                    "Only SELECT queries are allowed.")));

        await using CustomWebApplicationFactory factory = new(queryServiceMock: queryServiceMock);
        using HttpClient httpClient = factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "/api/data-sources/execute-query",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ApiErrorResponse? error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("datasources.query.only_select_allowed", error.Code);
    }

    [Fact]
    public async Task ExecuteQuery_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        Mock<IDataSourceQueryService> queryServiceMock = new();

        ExecuteDataSourceQueryRequest request = new(
            DataSourceType.Sqlite,
            "Data Source=:memory:",
            "SELECT Id, Email FROM Users");

        QueryExecutionResult queryResult = new(
            ["Id", "Email"],
            [
                new Dictionary<string, object?>
                {
                    ["Id"] = 1,
                    ["Email"] = "josey@sentra.dev"
                }
            ],
            1);

        queryServiceMock
            .Setup(service => service.ExecuteQueryAsync(
                request.DataSourceType,
                request.ConnectionString,
                request.Query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(queryResult));

        await using CustomWebApplicationFactory factory = new(queryServiceMock: queryServiceMock);
        using HttpClient httpClient = factory.CreateClient();

        // Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "/api/data-sources/execute-query",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}