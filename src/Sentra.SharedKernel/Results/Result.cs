namespace Sentra.SharedKernel.Results;

/// <summary>
/// Represents the outcome of an operation without a return value.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the result is successful.</param>
    /// <param name="error">The associated error.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a successful result contains an error or a failed result contains no error.
    /// </exception>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A successful result cannot contain an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("A failed result must contain an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the result is successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the associated error.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful non-generic result.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failed non-generic result.
    /// </summary>
    /// <param name="error">The associated error.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a successful generic result.
    /// </summary>
    /// <typeparam name="TValue">The result value type.</typeparam>
    /// <param name="value">The successful value.</param>
    /// <returns>A successful <see cref="Result{TValue}"/>.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed generic result.
    /// </summary>
    /// <typeparam name="TValue">The result value type.</typeparam>
    /// <param name="error">The associated error.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Represents the outcome of an operation with a return value.
/// </summary>
/// <typeparam name="TValue">The result value type.</typeparam>
public sealed class Result<TValue> : Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <param name="isSuccess">Indicates whether the result is successful.</param>
    /// <param name="error">The associated error.</param>
    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the result value.
    /// </summary>
    public TValue? Value { get; }

    /// <summary>
    /// Returns the value if the result is successful; otherwise throws an exception.
    /// </summary>
    /// <returns>The successful result value.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the result is failed.
    /// </exception>
    public TValue ValueOrThrow()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException("Cannot access the value of a failed result.");
        }

        return Value!;
    }
}