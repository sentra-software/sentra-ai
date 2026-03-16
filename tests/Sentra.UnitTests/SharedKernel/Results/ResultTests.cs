using FluentAssertions;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.SharedKernel.Results;

/// <summary>
/// Contains unit tests for <see cref="Result"/> and <see cref="Result{TValue}"/>.
/// </summary>
public sealed class ResultTests
{
    /// <summary>
    /// Verifies that a successful result is created correctly.
    /// </summary>
    [Fact]
    public void Success_Should_Create_Successful_Result()
    {
        Result? result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    /// <summary>
    /// Verifies that a failed result is created correctly.
    /// </summary>
    [Fact]
    public void Failure_Should_Create_Failed_Result()
    {
        Error? error = Error.Failure("failure", "Something failed.");

        Result? result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    /// <summary>
    /// Verifies that a successful generic result contains its value.
    /// </summary>
    [Fact]
    public void Generic_Success_Should_Contain_Value()
    {
        Result<int>? result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.ValueOrThrow().Should().Be(42);
    }

    /// <summary>
    /// Verifies that a failed generic result preserves its error.
    /// </summary>
    [Fact]
    public void Generic_Failure_Should_Not_Contain_Usable_Value()
    {
        Error? error = Error.Failure("failure", "Something failed.");

        Result<int>? result = Result.Failure<int>(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Value.Should().Be(0);
    }

    /// <summary>
    /// Verifies that <see cref="Result{TValue}.ValueOrThrow"/> throws for failed results.
    /// </summary>
    [Fact]
    public void ValueOrThrow_Should_Throw_When_Result_Is_Failure()
    {
        Result<int>? result = Result.Failure<int>(Error.Failure("failure", "Something failed."));

        Func<int>? action = () => result.ValueOrThrow();

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot access the value of a failed result.");
    }

    /// <summary>
    /// Verifies that a successful result cannot contain an error.
    /// </summary>
    [Fact]
    public void Constructor_Should_Throw_When_Success_Has_Error()
    {
        Func<TestResult>? action = () => new TestResult(true, Error.Failure("failure", "Invalid state."));

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("A successful result cannot contain an error.");
    }

    /// <summary>
    /// Verifies that a failed result must contain an error.
    /// </summary>
    [Fact]
    public void Constructor_Should_Throw_When_Failure_Has_None_Error()
    {
        Func<TestResult>? action = () => new TestResult(false, Error.None);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("A failed result must contain an error.");
    }

    /// <summary>
    /// Test helper type for exercising the protected <see cref="Result"/> constructor.
    /// </summary>
    private sealed class TestResult : Result
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestResult"/> class.
        /// </summary>
        /// <param name="isSuccess">Indicates whether the result is successful.</param>
        /// <param name="error">The associated error.</param>
        public TestResult(bool isSuccess, Error error)
            : base(isSuccess, error)
        {
        }
    }
}