using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Sqlite;
using Xunit;

namespace Sentra.UnitTests.Connectors.Sqlite;

/// <summary>
/// Contains tests for <see cref="SqliteConnector"/>.
/// </summary>
public sealed class SqliteConnectorTests
{
    private readonly SqliteConnector _connector = new();

    [Fact]
    public void Type_ShouldBeSqlite()
    {
        Assert.Equal(ConnectorType.Sqlite, _connector.Type);
    }

    [Fact]
    public void Capabilities_ShouldContainExpectedValues()
    {
        Assert.Contains(ConnectorCapability.TestConnection, _connector.Capabilities);
        Assert.Contains(ConnectorCapability.ReadSchema, _connector.Capabilities);
        Assert.Contains(ConnectorCapability.ExecuteQuery, _connector.Capabilities);
        Assert.Equal(3, _connector.Capabilities.Count);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldReturnFailure_ForClearlyInvalidConnectionString()
    {
        ConnectionTestResult result = await _connector.TestConnectionAsync(
            "This is not a connection string",
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldSucceed_ForInMemoryDatabase()
    {
        ConnectionTestResult result = await _connector.TestConnectionAsync("Data Source=:memory:", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Connection succeeded.", result.Message);
    }

    [Fact]
    public async Task ReadSchemaAsync_ShouldReturnCreatedTable_ForInMemoryDatabase()
    {
        string connectionString = "Data Source=file:sentra-schema-test?mode=memory&cache=shared";

        await using Microsoft.Data.Sqlite.SqliteConnection setupConnection = new(connectionString);
        await setupConnection.OpenAsync();

        await using (Microsoft.Data.Sqlite.SqliteCommand command = setupConnection.CreateCommand())
        {
            command.CommandText = """
                                  CREATE TABLE Users (
                                      Id INTEGER NOT NULL,
                                      Email TEXT NOT NULL,
                                      DisplayName TEXT NULL
                                  );
                                  """;
            await command.ExecuteNonQueryAsync();
        }

        IReadOnlyCollection<Sentra.Connectors.Abstractions.Schema.TableSchema> schema =
            await _connector.ReadSchemaAsync(connectionString, CancellationToken.None);

        Sentra.Connectors.Abstractions.Schema.TableSchema usersTable =
            Assert.Single(schema, table => table.Name == "Users");

        Assert.Equal("main", usersTable.Schema);
        Assert.Equal(3, usersTable.Columns.Count);
        Assert.Contains(usersTable.Columns, column => column.Name == "Id");
        Assert.Contains(usersTable.Columns, column => column.Name == "Email");
        Assert.Contains(usersTable.Columns, column => column.Name == "DisplayName");
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldReturnColumnsRowsAndRowCount_ForInMemoryDatabase()
    {
        string connectionString = "Data Source=file:sentra-query-test?mode=memory&cache=shared";

        await using Microsoft.Data.Sqlite.SqliteConnection setupConnection = new(connectionString);
        await setupConnection.OpenAsync();

        await using (Microsoft.Data.Sqlite.SqliteCommand command = setupConnection.CreateCommand())
        {
            command.CommandText = """
                                  CREATE TABLE Users (
                                      Id INTEGER NOT NULL,
                                      Email TEXT NOT NULL,
                                      DisplayName TEXT NULL
                                  );

                                  INSERT INTO Users (Id, Email, DisplayName)
                                  VALUES (1, 'josey@sentra.dev', NULL);
                                  """;
            await command.ExecuteNonQueryAsync();
        }

        Sentra.Connectors.Abstractions.Querying.QueryExecutionResult result =
            await _connector.ExecuteQueryAsync(
                connectionString,
                "SELECT Id, Email, DisplayName FROM Users",
                CancellationToken.None);

        Assert.Equal(3, result.Columns.Count);
        Assert.Contains("Id", result.Columns);
        Assert.Contains("Email", result.Columns);
        Assert.Contains("DisplayName", result.Columns);
        Assert.Equal(1, result.RowCount);

        IReadOnlyDictionary<string, object?> row = Assert.Single(result.Rows);
        Assert.Equal(1L, row["Id"]);
        Assert.Equal("josey@sentra.dev", row["Email"]);
        Assert.Null(row["DisplayName"]);
    }

    [Fact]
    public async Task ExecuteQueryAsync_ShouldReturnEmptyRows_ForQueryWithoutMatches()
    {
        string connectionString = "Data Source=file:sentra-empty-query-test?mode=memory&cache=shared";

        await using Microsoft.Data.Sqlite.SqliteConnection setupConnection = new(connectionString);
        await setupConnection.OpenAsync();

        await using (Microsoft.Data.Sqlite.SqliteCommand command = setupConnection.CreateCommand())
        {
            command.CommandText = """
                                  CREATE TABLE Users (
                                      Id INTEGER NOT NULL,
                                      Email TEXT NOT NULL
                                  );
                                  """;
            await command.ExecuteNonQueryAsync();
        }

        Sentra.Connectors.Abstractions.Querying.QueryExecutionResult result =
            await _connector.ExecuteQueryAsync(
                connectionString,
                "SELECT Id, Email FROM Users",
                CancellationToken.None);

        Assert.Equal(2, result.Columns.Count);
        Assert.Empty(result.Rows);
        Assert.Equal(0, result.RowCount);
    }
}