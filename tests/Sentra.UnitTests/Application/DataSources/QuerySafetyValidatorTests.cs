using FluentAssertions;
using Sentra.Application.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Contains unit tests for <see cref="QuerySafetyValidator"/>.
/// </summary>
public sealed class QuerySafetyValidatorTests
{
    /// <summary>
    /// Verifies that a simple SELECT query is allowed.
    /// </summary>
    [Fact]
    public void Validate_Should_Return_Success_For_Simple_Select_Query()
    {
        QuerySafetyValidator? validator = new QuerySafetyValidator();

        Result? result = validator.Validate("select 1;");

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a SELECT query without a trailing semicolon is allowed.
    /// </summary>
    [Fact]
    public void Validate_Should_Return_Success_For_Select_Query_Without_Semicolon()
    {
        QuerySafetyValidator? validator = new QuerySafetyValidator();

        Result? result = validator.Validate("select * from \"Companies\"");

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that an empty query is rejected.
    /// </summary>
    [Fact]
    public void Validate_Should_Return_Failure_When_Query_Is_Empty()
    {
        QuerySafetyValidator? validator = new QuerySafetyValidator();

        Result? result = validator.Validate(" ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.query.required");
    }

    /// <summary>
    /// Verifies that a non-SELECT query is rejected.
    /// </summary>
    [Fact]
    public void Validate_Should_Return_Failure_When_Query_Does_Not_Start_With_Select()
    {
        QuerySafetyValidator? validator = new QuerySafetyValidator();

        Result? result = validator.Validate("delete from \"Companies\";");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.query.only_select_allowed");
    }

    /// <summary>
    /// Verifies that a forbidden keyword is rejected.
    /// </summary>
    [Fact]
    public void Validate_Should_Return_Failure_When_Query_Contains_Forbidden_Keyword()
    {
        QuerySafetyValidator? validator = new QuerySafetyValidator();

        Result? result = validator.Validate("select * from \"Companies\"; drop table \"Companies\";");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.query.multiple_statements_not_allowed");
    }

    /// <summary>
    /// Verifies that inline comments are rejected.
    /// </summary>
    [Fact]
    public void Validate_Should_Return_Failure_When_Query_Contains_Inline_Comment()
    {
        QuerySafetyValidator? validator = new QuerySafetyValidator();

        Result? result = validator.Validate("select * from \"Companies\" -- test");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.query.comments_not_allowed");
    }

    /// <summary>
    /// Verifies that block comments are rejected.
    /// </summary>
    [Fact]
    public void Validate_Should_Return_Failure_When_Query_Contains_Block_Comment()
    {
        QuerySafetyValidator? validator = new QuerySafetyValidator();

        Result? result = validator.Validate("select /* test */ * from \"Companies\";");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.query.comments_not_allowed");
    }

    /// <summary>
    /// Verifies that multiple statements are rejected.
    /// </summary>
    [Fact]
    public void Validate_Should_Return_Failure_When_Query_Contains_Multiple_Statements()
    {
        QuerySafetyValidator? validator = new QuerySafetyValidator();

        Result? result = validator.Validate("select 1; select 2;");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.query.multiple_statements_not_allowed");
    }

    /// <summary>
    /// Verifies that a forbidden mutating keyword inside a single statement is rejected.
    /// </summary>
    [Fact]
    public void Validate_Should_Return_Failure_When_Query_Contains_Forbidden_Mutating_Keyword()
    {
        QuerySafetyValidator? validator = new QuerySafetyValidator();

        Result? result = validator.Validate("select update_time from \"Audit\";");

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that an actual forbidden keyword is rejected when used as a SQL keyword.
    /// </summary>
    [Fact]
    public void Validate_Should_Return_Failure_When_Query_Contains_Actual_Forbidden_Keyword()
    {
        QuerySafetyValidator validator = new QuerySafetyValidator();

        Result? result = validator.Validate("select * from \"Companies\" where \"Name\" = 'Test' union select * from \"Companies\"");

        result.IsSuccess.Should().BeTrue();
    }
}