using Xunit;

using Sentra.Application.DataSources;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Contains tests for <see cref="QuerySafetyValidator"/>.
/// </summary>
public sealed class QuerySafetyValidatorTests
{
    private readonly QuerySafetyValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\r\n")]
    [InlineData("\t")]
    public void Validate_ShouldFail_WhenQueryIsNullOrWhitespace(string? query)
    {
        // Act
        var result = _validator.Validate(query!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("datasources.query.required", result.Error.Code);
    }

    [Theory]
    [InlineData("DELETE FROM Users")]
    [InlineData("UPDATE Users SET Name = 'Josey'")]
    [InlineData("INSERT INTO Users(Name) VALUES ('Josey')")]
    [InlineData("DROP TABLE Users")]
    [InlineData("ALTER TABLE Users ADD COLUMN Test int")]
    [InlineData("EXEC sp_help")]
    [InlineData("CALL SomeProcedure()")]
    public void Validate_ShouldFail_WhenQueryDoesNotStartWithSelectOrWith(string query)
    {
        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("datasources.query.only_select_allowed", result.Error.Code);
    }

    [Theory]
    [InlineData("SELECT * FROM Users")]
    [InlineData(" select * from users ")]
    [InlineData("SELECT Id, Name FROM Companies")]
    [InlineData("WITH cte AS (SELECT 1 AS Value) SELECT * FROM cte")]
    [InlineData("WITH users_cte AS (SELECT Id FROM Users) SELECT * FROM users_cte")]
    [InlineData("SELECT * FROM Users;")]
    [InlineData("SELECT * FROM Users;   ")]
    [InlineData("SELECT * FROM Users ;")]
    [InlineData("SELECT * FROM Users ;   ")]
    [InlineData("WITH cte AS (SELECT 1 AS Value) SELECT * FROM cte;")]
    public void Validate_ShouldSucceed_ForSafeSingleSelectQueries(string query)
    {
        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("SELECT * FROM Users -- comment")]
    [InlineData("SELECT * FROM Users /* comment */")]
    [InlineData("SELECT * FROM Users /* comment")]
    [InlineData("SELECT * FROM Users */")]
    public void Validate_ShouldFail_WithCommentsNotAllowed_WhenQueryStartsAsSelectOrWith(string query)
    {
        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("datasources.query.comments_not_allowed", result.Error.Code);
    }

    [Fact]
    public void Validate_ShouldFail_WithOnlySelectAllowed_WhenQueryStartsWithComment()
    {
        // Arrange
        const string query = "-- comment\r\nSELECT * FROM Users";

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("datasources.query.only_select_allowed", result.Error.Code);
    }

    [Theory]
    [InlineData("SELECT * FROM Users; SELECT * FROM Roles")]
    [InlineData("SELECT * FROM Users; DELETE FROM Users")]
    [InlineData("WITH cte AS (SELECT 1) SELECT * FROM cte; SELECT 2")]
    [InlineData("SELECT * FROM Users;SELECT * FROM Roles")]
    [InlineData("SELECT * FROM Users;SELECT * FROM Roles;")]
    [InlineData("SELECT 1; SELECT 2; SELECT 3;")]
    public void Validate_ShouldFail_WhenQueryContainsMultipleStatements(string query)
    {
        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("datasources.query.multiple_statements_not_allowed", result.Error.Code);
    }

    [Theory]
    [InlineData("SELECT * INTO backup_users FROM Users", "INTO")]
    [InlineData("SELECT * FROM Users WHERE Notes = 'DROP TABLE Users'", "DROP")]
    [InlineData("SELECT * FROM Users WHERE Notes = 'DELETE FROM Users'", "DELETE")]
    [InlineData("SELECT * FROM Users WHERE Notes = 'EXEC test'", "EXEC")]
    [InlineData("SELECT * FROM Users WHERE Notes = 'PRAGMA foreign_keys = OFF'", "PRAGMA")]
    [InlineData("SELECT * FROM Users WHERE Name = 'into'", "INTO")]
    [InlineData("SELECT * FROM Users WHERE Name = 'drop'", "DROP")]
    [InlineData("SELECT * FROM Users WHERE Name = 'exec'", "EXEC")]
    public void Validate_ShouldFail_WhenForbiddenKeywordIsPresent(string query, string expectedKeyword)
    {
        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("datasources.query.forbidden_keyword", result.Error.Code);
        Assert.Contains(expectedKeyword, result.Error.Message);
    }

    [Theory]
    [InlineData("SELECT INSERTED_AT FROM AuditLogs")]
    [InlineData("SELECT UPDATED_BY FROM AuditLogs")]
    [InlineData("SELECT DELETED_FLAG FROM AuditLogs")]
    [InlineData("SELECT DROPPED_COUNT FROM Metrics")]
    [InlineData("SELECT EXECUTION_TIME FROM Metrics")]
    [InlineData("SELECT CALLSIGN FROM Contacts")]
    [InlineData("SELECT PRAGMATIC_VALUE FROM Settings")]
    [InlineData("WITH cte AS (SELECT 1) SELECT EXECUTE_VALUE FROM cte")]
    public void Validate_ShouldSucceed_WhenForbiddenKeywordIsOnlyPartOfAnotherWord(string query)
    {
        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_ShouldAllow_SelectQueryWithoutSemicolon()
    {
        // Arrange
        const string query = "SELECT * FROM Users";

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_ShouldAllow_WithQueryUsingSingleTrailingSemicolon()
    {
        // Arrange
        const string query = "WITH cte AS (SELECT 1 AS Value) SELECT * FROM cte;";

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.True(result.IsSuccess);
    }
}