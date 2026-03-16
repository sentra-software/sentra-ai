using FluentAssertions;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.SharedKernel.Results;

/// <summary>
/// Contains unit tests for the <see cref="Error"/> type.
/// </summary>
public sealed class ErrorTests
{
    /// <summary>
    /// Verifies that <see cref="Error.None"/> has empty values.
    /// </summary>
    [Fact]
    public void None_Should_Have_Empty_Code_And_Message()
    {
        Error.None.Code.Should().BeEmpty();
        Error.None.Message.Should().BeEmpty();
        Error.None.IsNone.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that validation errors preserve the provided values.
    /// </summary>
    [Fact]
    public void Validation_Should_Create_Error_With_Provided_Values()
    {
        Error? error = Error.Validation("validation.required", "Name is required.");

        error.Code.Should().Be("validation.required");
        error.Message.Should().Be("Name is required.");
        error.IsNone.Should().BeFalse();
    }

    /// <summary>
    /// Verifies the textual representation of an error.
    /// </summary>
    [Fact]
    public void ToString_Should_Return_Code_And_Message()
    {
        Error? error = Error.Failure("general.failure", "Something went wrong.");

        error.ToString().Should().Be("general.failure: Something went wrong.");
    }
}