using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

using Sentra.Api.Controllers;
using Sentra.Api.Models;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Contracts.DataSources;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Api.Controllers;

/// <summary>
/// Contains tests for <see cref="DataSourcesController"/>.
/// </summary>
public sealed class DataSourcesControllerTests
{
    [Fact]
    public async Task TestConnection_ShouldReturnBadRequest_WhenServiceFails()
    {
        DataSourcesController controller = new();
        Mock<IDataSourceConnectionService> serviceMock = new();

        TestDataSourceConnectionRequest request = new(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;");

        serviceMock
            .Setup(service => service.TestConnectionAsync(
                request.DataSourceType,
                request.ConnectionString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ConnectionTestResult>(
                Error.Validation("datasources.connection_string.required", "Connection string is required.")));

        IActionResult actionResult = await controller.TestConnection(request, serviceMock.Object, CancellationToken.None);

        BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        ApiErrorResponse response = Assert.IsType<ApiErrorResponse>(badRequest.Value);

        Assert.Equal("datasources.connection_string.required", response.Code);
    }

    [Fact]
    public async Task TestConnection_ShouldReturnOk_WhenServiceSucceeds()
    {
        DataSourcesController controller = new();
        Mock<IDataSourceConnectionService> serviceMock = new();

        TestDataSourceConnectionRequest request = new(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;");

        serviceMock
            .Setup(service => service.TestConnectionAsync(
                request.DataSourceType,
                request.ConnectionString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ConnectionTestResult.Success("Connection succeeded.")));

        IActionResult actionResult = await controller.TestConnection(request, serviceMock.Object, CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task ReadSchema_ShouldReturnBadRequest_WhenServiceFails()
    {
        DataSourcesController controller = new();
        Mock<IDataSourceSchemaService> serviceMock = new();

        ReadDataSourceSchemaRequest request = new(
            DataSourceType.Sqlite,
            "Data Source=:memory:");

        serviceMock
            .Setup(service => service.ReadSchemaAsync(
                request.DataSourceType,
                request.ConnectionString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<IReadOnlyCollection<TableSchema>>(
                Error.Failure("connectors.not_registered", "Connector is not registered.")));

        IActionResult actionResult = await controller.ReadSchema(request, serviceMock.Object, CancellationToken.None);

        BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        ApiErrorResponse response = Assert.IsType<ApiErrorResponse>(badRequest.Value);

        Assert.Equal("connectors.not_registered", response.Code);
    }

    [Fact]
    public async Task ReadSchema_ShouldReturnOk_WhenServiceSucceeds()
    {
        DataSourcesController controller = new();
        Mock<IDataSourceSchemaService> serviceMock = new();

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

        serviceMock
            .Setup(service => service.ReadSchemaAsync(
                request.DataSourceType,
                request.ConnectionString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(schema));

        IActionResult actionResult = await controller.ReadSchema(request, serviceMock.Object, CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task ExecuteQuery_ShouldReturnBadRequest_WhenServiceFails()
    {
        DataSourcesController controller = new();
        Mock<IDataSourceQueryService> serviceMock = new();

        ExecuteDataSourceQueryRequest request = new(
            DataSourceType.Sqlite,
            "Data Source=:memory:",
            "DELETE FROM Users");

        serviceMock
            .Setup(service => service.ExecuteQueryAsync(
                request.DataSourceType,
                request.ConnectionString,
                request.Query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<QueryExecutionResult>(
                Error.Validation("datasources.query.only_select_allowed", "Only SELECT queries are allowed.")));

        IActionResult actionResult = await controller.ExecuteQuery(request, serviceMock.Object, CancellationToken.None);

        BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        ApiErrorResponse response = Assert.IsType<ApiErrorResponse>(badRequest.Value);

        Assert.Equal("datasources.query.only_select_allowed", response.Code);
    }

    [Fact]
    public async Task ExecuteQuery_ShouldReturnOk_WhenServiceSucceeds()
    {
        DataSourcesController controller = new();
        Mock<IDataSourceQueryService> serviceMock = new();

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

        serviceMock
            .Setup(service => service.ExecuteQueryAsync(
                request.DataSourceType,
                request.ConnectionString,
                request.Query,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(queryResult));

        IActionResult actionResult = await controller.ExecuteQuery(request, serviceMock.Object, CancellationToken.None);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(actionResult);
        Assert.NotNull(ok.Value);
    }
}