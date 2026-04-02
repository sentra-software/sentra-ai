using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.MySql;
using Xunit;

namespace Sentra.UnitTests.Connectors.MySql;

/// <summary>
/// Contains tests for <see cref="MySqlConnector"/>.
/// </summary>
public sealed class MySqlConnectorTests
{
    private readonly Sentra.Connectors.MySql.MySqlConnector _connector = new();

    [Fact]
    public void Type_ShouldBeMySql()
    {
        Assert.Equal(ConnectorType.MySql, _connector.Type);
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
    [InlineData("Server=invalid-host;Port=3306;Database=test;User Id=test;Password=test;")]
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
            _connector.ReadSchemaAsync("Server=invalid-host;Port=3306;Database=test;User Id=test;Password=test;", CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldThrow_ForInvalidConnectionString()
    {
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _connector.ExecuteQueryAsync(
                "Server=invalid-host;Port=3306;Database=test;User Id=test;Password=test;",
                "SELECT 1",
                CancellationToken.None));
    }
}