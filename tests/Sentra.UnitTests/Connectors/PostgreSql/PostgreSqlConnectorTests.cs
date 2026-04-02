using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.PostgreSql;
using Xunit;

namespace Sentra.UnitTests.Connectors.PostgreSql;

/// <summary>
/// Contains tests for <see cref="PostgreSqlConnector"/>.
/// </summary>
public sealed class PostgreSqlConnectorTests
{
    private readonly PostgreSqlConnector _connector = new();

    [Fact]
    public void Type_ShouldBePostgreSql()
    {
        Assert.Equal(ConnectorType.PostgreSql, _connector.Type);
    }

    [Fact]
    public void Capabilities_ShouldContainExpectedValues()
    {
        Assert.Contains(ConnectorCapability.TestConnection, _connector.Capabilities);
        Assert.Contains(ConnectorCapability.ReadSchema, _connector.Capabilities);
        Assert.Contains(ConnectorCapability.ExecuteQuery, _connector.Capabilities);
        Assert.Equal(3, _connector.Capabilities.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Host=invalid-host;Port=5432;Database=test;Username=test;Password=test;")]
    [InlineData("This is not a connection string")]
    public async Task TestConnectionAsync_ShouldReturnFailure_ForInvalidConnectionStrings(string connectionString)
    {
        ConnectionTestResult result = await _connector.TestConnectionAsync(connectionString, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    [Fact]
    public async Task ReadSchemaAsync_ShouldThrow_ForInvalidConnectionString()
    {
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _connector.ReadSchemaAsync("Host=invalid-host;Port=5432;Database=test;Username=test;Password=test;", CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldThrow_ForInvalidConnectionString()
    {
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _connector.ExecuteQueryAsync(
                "Host=invalid-host;Port=5432;Database=test;Username=test;Password=test;",
                "SELECT 1",
                CancellationToken.None));
    }
}