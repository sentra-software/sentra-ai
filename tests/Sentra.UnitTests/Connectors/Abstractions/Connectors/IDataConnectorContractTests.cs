using FluentAssertions;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.UnitTests.Connectors.Abstractions.Connectors;

/// <summary>
/// Contains contract-oriented tests for <see cref="IDataConnector"/>.
/// </summary>
public sealed class IDataConnectorContractTests
{
    /// <summary>
    /// Verifies that the test connector exposes the expected type and capabilities.
    /// </summary>
    [Fact]
    public void Connector_Should_Expose_Metadata()
    {
        IDataConnector connector = new TestDataConnector();

        connector.Type.Should().Be(ConnectorType.PostgreSql);
        connector.Capabilities.Should().Contain(ConnectorCapability.TestConnection);
        connector.Capabilities.Should().Contain(ConnectorCapability.ReadSchema);
        connector.Capabilities.Should().Contain(ConnectorCapability.ExecuteQuery);
    }

    /// <summary>
    /// Verifies that the connector can return a connection test result.
    /// </summary>
    [Fact]
    public async Task TestConnectionAsync_Should_Return_Result()
    {
        IDataConnector connector = new TestDataConnector();

        ConnectionTestResult? result = await connector.TestConnectionAsync("Host=localhost;");

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("OK");
    }

    /// <summary>
    /// Verifies that the connector can return schema information.
    /// </summary>
    [Fact]
    public async Task ReadSchemaAsync_Should_Return_Tables()
    {
        IDataConnector connector = new TestDataConnector();

        IReadOnlyCollection<TableSchema>? result = await connector.ReadSchemaAsync("Host=localhost;");

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("customers");
    }

    /// <summary>
    /// Verifies that the connector can return query results.
    /// </summary>
    [Fact]
    public async Task ExecuteQueryAsync_Should_Return_Query_Result()
    {
        IDataConnector connector = new TestDataConnector();

        QueryExecutionResult? result = await connector.ExecuteQueryAsync("Host=localhost;", "select 1");

        result.RowCount.Should().Be(1);
        result.Columns.Should().Contain("id");
    }
}