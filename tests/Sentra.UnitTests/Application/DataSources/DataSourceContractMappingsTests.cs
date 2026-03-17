using FluentAssertions;
using Sentra.Application.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Contracts.DataSources;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Contains unit tests for <see cref="DataSourceContractMappings"/>.
/// </summary>
public sealed class DataSourceContractMappingsTests
{
    /// <summary>
    /// Verifies that a connection test result is mapped correctly.
    /// </summary>
    [Fact]
    public void ToResponse_Should_Map_ConnectionTestResult()
    {
        ConnectionTestResult? result = ConnectionTestResult.Success("Connection succeeded.");

        TestDataSourceConnectionResponse? response = result.ToResponse();

        response.IsSuccess.Should().BeTrue();
        response.Message.Should().Be("Connection succeeded.");
    }

    /// <summary>
    /// Verifies that table schemas are mapped correctly.
    /// </summary>
    [Fact]
    public void ToResponse_Should_Map_TableSchemas()
    {
        IReadOnlyCollection<TableSchema> tables =
        [
            new TableSchema(
                "public",
                "customers",
                new[]
                {
                    new ColumnSchema("id", "uuid", false),
                    new ColumnSchema("name", "text", true)
                })
        ];

        ReadDataSourceSchemaResponse? response = tables.ToResponse();

        response.Tables.Should().HaveCount(1);
        response.Tables.First().Schema.Should().Be("public");
        response.Tables.First().Name.Should().Be("customers");
        response.Tables.First().Columns.Should().HaveCount(2);
        response.Tables.First().Columns.First().Name.Should().Be("id");
    }

    /// <summary>
    /// Verifies that a query execution result is mapped correctly.
    /// </summary>
    [Fact]
    public void ToResponse_Should_Map_QueryExecutionResult()
    {
        QueryExecutionResult? result = new QueryExecutionResult(
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

        ExecuteDataSourceQueryResponse? response = result.ToResponse();

        response.Columns.Should().ContainInOrder("id", "name");
        response.RowCount.Should().Be(1);
        response.Rows.Should().HaveCount(1);
        response.Rows.First().Values["id"].Should().Be(1);
        response.Rows.First().Values["name"].Should().Be("Josey");
    }

    /// <summary>
    /// Verifies that mapping a null connection result throws.
    /// </summary>
    [Fact]
    public void ToResponse_Should_Throw_When_ConnectionTestResult_Is_Null()
    {
        ConnectionTestResult? result = null;

        Func<TestDataSourceConnectionResponse>? action = () => result!.ToResponse();

        action.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that mapping null tables throws.
    /// </summary>
    [Fact]
    public void ToResponse_Should_Throw_When_TableSchemas_Are_Null()
    {
        IReadOnlyCollection<TableSchema>? tables = null;

        Func<ReadDataSourceSchemaResponse>? action = () => tables!.ToResponse();

        action.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that mapping a null query result throws.
    /// </summary>
    [Fact]
    public void ToResponse_Should_Throw_When_QueryExecutionResult_Is_Null()
    {
        QueryExecutionResult? result = null;

        Func<ExecuteDataSourceQueryResponse>? action = () => result!.ToResponse();

        action.Should().Throw<ArgumentNullException>();
    }
}