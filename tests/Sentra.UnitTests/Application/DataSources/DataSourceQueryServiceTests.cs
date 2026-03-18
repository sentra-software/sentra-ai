using FluentAssertions;
using Sentra.Application.Connectors;
using Sentra.Application.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Contains unit tests for <see cref="DataSourceQueryService"/>.
/// </summary>
public sealed class DataSourceQueryServiceTests
{
    /// <summary>
    /// Verifies that a PostgreSQL data source uses the PostgreSQL connector and returns query results.
    /// </summary>
    [Fact]
    public async Task ExecuteQueryAsync_Should_Use_PostgreSql_Connector_For_PostgreSql_DataSource()
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

        TestQueryDataConnector? connector = new TestQueryDataConnector(
            ConnectorType.PostgreSql,
            queryResult);

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        TestQuerySafetyValidator? querySafetyValidator = new TestQuerySafetyValidator(Result.Success());
        DataSourceQueryService? service = new DataSourceQueryService(registry, querySafetyValidator);

        Result<QueryExecutionResult>? result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;",
            "select id, name from users;");

        result.IsSuccess.Should().BeTrue();
        result.ValueOrThrow().RowCount.Should().Be(1);
        result.ValueOrThrow().Columns.Should().ContainInOrder("id", "name");
        connector.LastConnectionString.Should().Be("Host=localhost;Database=sentra;");
        connector.LastQuery.Should().Be("select id, name from users;");
        querySafetyValidator.LastQuery.Should().Be("select id, name from users;");
    }

    /// <summary>
    /// Verifies that the connection string and query are trimmed before execution.
    /// </summary>
    [Fact]
    public async Task ExecuteQueryAsync_Should_Trim_Connection_String_And_Query()
    {
        QueryExecutionResult? queryResult = new QueryExecutionResult(
            Array.Empty<string>(),
            Array.Empty<IReadOnlyDictionary<string, object?>>(),
            0);

        TestQueryDataConnector? connector = new TestQueryDataConnector(
            ConnectorType.PostgreSql,
            queryResult);

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        TestQuerySafetyValidator? querySafetyValidator = new TestQuerySafetyValidator(Result.Success());
        DataSourceQueryService? service = new DataSourceQueryService(registry, querySafetyValidator);

        Result<QueryExecutionResult>? result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            "  Host=localhost;Database=sentra;  ",
            "  select 1;  ");

        result.IsSuccess.Should().BeTrue();
        connector.LastConnectionString.Should().Be("Host=localhost;Database=sentra;");
        connector.LastQuery.Should().Be("select 1;");
        querySafetyValidator.LastQuery.Should().Be("  select 1;  ");
    }

    /// <summary>
    /// Verifies that an empty connection string is rejected.
    /// </summary>
    [Fact]
    public async Task ExecuteQueryAsync_Should_Return_Failure_When_Connection_String_Is_Empty()
    {
        QueryExecutionResult? queryResult = new QueryExecutionResult(
            Array.Empty<string>(),
            Array.Empty<IReadOnlyDictionary<string, object?>>(),
            0);

        TestQueryDataConnector? connector = new TestQueryDataConnector(
            ConnectorType.PostgreSql,
            queryResult);

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        TestQuerySafetyValidator? querySafetyValidator = new TestQuerySafetyValidator(Result.Success());
        DataSourceQueryService? service = new DataSourceQueryService(registry, querySafetyValidator);

        Result<QueryExecutionResult>? result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            " ",
            "select 1;");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.connection_string.required");
    }

    /// <summary>
    /// Verifies that a failed query safety validation is returned.
    /// </summary>
    [Fact]
    public async Task ExecuteQueryAsync_Should_Return_Failure_When_Query_Safety_Validation_Fails()
    {
        QueryExecutionResult? queryResult = new QueryExecutionResult(
            Array.Empty<string>(),
            Array.Empty<IReadOnlyDictionary<string, object?>>(),
            0);

        TestQueryDataConnector? connector = new TestQueryDataConnector(
            ConnectorType.PostgreSql,
            queryResult);

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        TestQuerySafetyValidator? querySafetyValidator = new TestQuerySafetyValidator(
            Result.Failure(
                Error.Validation(
                    "datasources.query.only_select_allowed",
                    "Only SELECT queries are allowed.")));

        DataSourceQueryService? service = new DataSourceQueryService(registry, querySafetyValidator);

        Result<QueryExecutionResult>? result = await service.ExecuteQueryAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;",
            "delete from \"Companies\";");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.query.only_select_allowed");
        connector.LastQuery.Should().BeNull();
    }

    /// <summary>
    /// Verifies that an invalid data source type is rejected.
    /// </summary>
    [Fact]
    public async Task ExecuteQueryAsync_Should_Return_Failure_When_DataSourceType_Is_Invalid()
    {
        QueryExecutionResult? queryResult = new QueryExecutionResult(
            Array.Empty<string>(),
            Array.Empty<IReadOnlyDictionary<string, object?>>(),
            0);

        TestQueryDataConnector? connector = new TestQueryDataConnector(
            ConnectorType.PostgreSql,
            queryResult);

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        TestQuerySafetyValidator? querySafetyValidator = new TestQuerySafetyValidator(Result.Success());
        DataSourceQueryService? service = new DataSourceQueryService(registry, querySafetyValidator);

        Result<QueryExecutionResult>? result = await service.ExecuteQueryAsync(
            (DataSourceType)999,
            "Host=localhost;",
            "select 1;");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.type.invalid");
    }

    /// <summary>
    /// Verifies that a missing connector registration returns a failure.
    /// </summary>
    [Fact]
    public async Task ExecuteQueryAsync_Should_Return_Failure_When_Connector_Is_Not_Registered()
    {
        QueryExecutionResult? queryResult = new QueryExecutionResult(
            Array.Empty<string>(),
            Array.Empty<IReadOnlyDictionary<string, object?>>(),
            0);

        ConnectorRegistry? registry = new ConnectorRegistry(
        [
            new TestQueryDataConnector(
                ConnectorType.PostgreSql,
                queryResult)
        ]);

        TestQuerySafetyValidator? querySafetyValidator = new TestQuerySafetyValidator(Result.Success());
        DataSourceQueryService? service = new DataSourceQueryService(registry, querySafetyValidator);

        Result<QueryExecutionResult>? result = await service.ExecuteQueryAsync(
            DataSourceType.MySql,
            "Server=localhost;",
            "select 1;");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("connectors.not_registered");
    }

    /// <summary>
    /// Verifies that the constructor rejects a null connector registry.
    /// </summary>
    [Fact]
    public void Constructor_Should_Throw_When_ConnectorRegistry_Is_Null()
    {
        TestQuerySafetyValidator? querySafetyValidator = new TestQuerySafetyValidator(Result.Success());

        Func<DataSourceQueryService>? action = () => new DataSourceQueryService(null!, querySafetyValidator);

        action.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that the constructor rejects a null query safety validator.
    /// </summary>
    [Fact]
    public void Constructor_Should_Throw_When_QuerySafetyValidator_Is_Null()
    {
        ConnectorRegistry? registry = new ConnectorRegistry(
        [
            new TestQueryDataConnector(
                ConnectorType.PostgreSql,
                new QueryExecutionResult(
                    Array.Empty<string>(),
                    Array.Empty<IReadOnlyDictionary<string, object?>>(),
                    0))
        ]);

        Func<DataSourceQueryService>? action = () => new DataSourceQueryService(registry, null!);

        action.Should().Throw<ArgumentNullException>();
    }
}