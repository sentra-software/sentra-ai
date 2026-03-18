using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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
/// Contains unit tests for <see cref="DataSourcesController"/>.
/// </summary>
public sealed class DataSourcesControllerTests
{
    /// <summary>
    /// Verifies that <see cref="DataSourcesController.TestConnection"/> returns OK for a successful result.
    /// </summary>
    [Fact]
    public async Task TestConnection_Should_Return_Ok_When_Service_Succeeds()
    {
        DataSourcesController? controller = new DataSourcesController();
        TestDataSourceConnectionService? service = new TestDataSourceConnectionService(
            Result.Success(ConnectionTestResult.Success("Connection succeeded.")));

        TestDataSourceConnectionRequest? request = new TestDataSourceConnectionRequest(
            DataSourceType.PostgreSql,
            "Host=localhost;");

        IActionResult? result = await controller.TestConnection(
            request,
            service,
            CancellationToken.None);

        OkObjectResult? okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        TestDataSourceConnectionResponse? response = okResult.Value.Should().BeOfType<TestDataSourceConnectionResponse>().Subject;

        response.IsSuccess.Should().BeTrue();
        response.Message.Should().Be("Connection succeeded.");
    }

    /// <summary>
    /// Verifies that <see cref="DataSourcesController.TestConnection"/> returns BadRequest for a failed result.
    /// </summary>
    [Fact]
    public async Task TestConnection_Should_Return_BadRequest_When_Service_Fails()
    {
        DataSourcesController controller = new DataSourcesController();
        TestDataSourceConnectionService? service = new TestDataSourceConnectionService(
            Result.Failure<ConnectionTestResult>(
                Error.Validation("datasources.connection_string.required", "Connection string is required.")));

        TestDataSourceConnectionRequest? request = new TestDataSourceConnectionRequest(
            DataSourceType.PostgreSql,
            string.Empty);

        IActionResult? result = await controller.TestConnection(
            request,
            service,
            CancellationToken.None);

        BadRequestObjectResult? badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        ApiErrorResponse? response = badRequestResult.Value.Should().BeOfType<ApiErrorResponse>().Subject;

        response.Code.Should().Be("datasources.connection_string.required");
        response.Message.Should().Be("Connection string is required.");
    }

    /// <summary>
    /// Verifies that <see cref="DataSourcesController.ReadSchema"/> returns OK for a successful result.
    /// </summary>
    [Fact]
    public async Task ReadSchema_Should_Return_Ok_When_Service_Succeeds()
    {
        IReadOnlyCollection<TableSchema> tables =
        [
            new TableSchema(
                "public",
                "customers",
                new[]
                {
                    new ColumnSchema("id", "uuid", false)
                })
        ];

        DataSourcesController? controller = new DataSourcesController();
        TestDataSourceSchemaService? service = new TestDataSourceSchemaService(Result.Success(tables));

        ReadDataSourceSchemaRequest? request = new ReadDataSourceSchemaRequest(
            DataSourceType.PostgreSql,
            "Host=localhost;");

        IActionResult? result = await controller.ReadSchema(
            request,
            service,
            CancellationToken.None);

        OkObjectResult? okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        ReadDataSourceSchemaResponse? response = okResult.Value.Should().BeOfType<ReadDataSourceSchemaResponse>().Subject;

        response.Tables.Should().HaveCount(1);
        response.Tables.First().Schema.Should().Be("public");
        response.Tables.First().Name.Should().Be("customers");
        response.Tables.First().Columns.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies that <see cref="DataSourcesController.ReadSchema"/> returns BadRequest for a failed result.
    /// </summary>
    [Fact]
    public async Task ReadSchema_Should_Return_BadRequest_When_Service_Fails()
    {
        DataSourcesController? controller = new DataSourcesController();
        TestDataSourceSchemaService? service = new TestDataSourceSchemaService(
            Result.Failure<IReadOnlyCollection<TableSchema>>(
                Error.Validation("datasources.connection_string.required", "Connection string is required.")));

        ReadDataSourceSchemaRequest? request = new ReadDataSourceSchemaRequest(
            DataSourceType.PostgreSql,
            string.Empty);

        IActionResult? result = await controller.ReadSchema(
            request,
            service,
            CancellationToken.None);

        BadRequestObjectResult? badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        ApiErrorResponse? response = badRequestResult.Value.Should().BeOfType<ApiErrorResponse>().Subject;

        response.Code.Should().Be("datasources.connection_string.required");
        response.Message.Should().Be("Connection string is required.");
    }

    /// <summary>
    /// Verifies that <see cref="DataSourcesController.ExecuteQuery"/> returns OK for a successful result.
    /// </summary>
    [Fact]
    public async Task ExecuteQuery_Should_Return_Ok_When_Service_Succeeds()
    {
        QueryExecutionResult? queryResult = new QueryExecutionResult(
            new[] { "id", "name" },
            new[]
            {
                new Dictionary<string, object?>
                {
                    ["id"] = 1,
                    ["name"] = "Josey"
                }
            },
            1);

        DataSourcesController? controller = new DataSourcesController();
        TestDataSourceQueryService? service = new TestDataSourceQueryService(Result.Success(queryResult));

        ExecuteDataSourceQueryRequest? request = new ExecuteDataSourceQueryRequest(
            DataSourceType.PostgreSql,
            "Host=localhost;",
            "select id, name from users;");

        IActionResult? result = await controller.ExecuteQuery(
            request,
            service,
            CancellationToken.None);

        OkObjectResult? okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        ExecuteDataSourceQueryResponse? response = okResult.Value.Should().BeOfType<ExecuteDataSourceQueryResponse>().Subject;

        response.Columns.Should().ContainInOrder("id", "name");
        response.RowCount.Should().Be(1);
        response.Rows.Should().HaveCount(1);
        response.Rows.First().Values["name"].Should().Be("Josey");
    }

    /// <summary>
    /// Verifies that <see cref="DataSourcesController.ExecuteQuery"/> returns BadRequest for a failed result.
    /// </summary>
    [Fact]
    public async Task ExecuteQuery_Should_Return_BadRequest_When_Service_Fails()
    {
        DataSourcesController? controller = new DataSourcesController();
        TestDataSourceQueryService? service = new TestDataSourceQueryService(
            Result.Failure<QueryExecutionResult>(
                Error.Validation("datasources.query.required", "Query is required.")));

        ExecuteDataSourceQueryRequest? request = new ExecuteDataSourceQueryRequest(
            DataSourceType.PostgreSql,
            "Host=localhost;",
            string.Empty);

        IActionResult? result = await controller.ExecuteQuery(
            request,
            service,
            CancellationToken.None);

        BadRequestObjectResult? badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        ApiErrorResponse? response = badRequestResult.Value.Should().BeOfType<ApiErrorResponse>().Subject;

        response.Code.Should().Be("datasources.query.required");
        response.Message.Should().Be("Query is required.");
    }
}