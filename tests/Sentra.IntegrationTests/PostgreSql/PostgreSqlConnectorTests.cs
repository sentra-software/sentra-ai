using FluentAssertions;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.PostgreSql;

namespace Sentra.IntegrationTests.PostgreSql;

/// <summary>
/// Contains integration tests for the PostgreSQL connector.
/// </summary>
public sealed class PostgreSqlConnectorTests
{
    /// <summary>
    /// Tests that the connector can connect to PostgreSQL.
    /// </summary>
    [Fact]
    public async Task TestConnection_Should_Succeed()
    {
        string? connectionString =
            "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";

        PostgreSqlConnector? connector = new PostgreSqlConnector();

        ConnectionTestResult? result = await connector.TestConnectionAsync(connectionString);

        result.IsSuccess.Should().BeTrue();
    }
}