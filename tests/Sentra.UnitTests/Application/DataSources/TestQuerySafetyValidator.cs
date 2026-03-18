using Sentra.Application.Abstractions.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Represents a configurable test implementation of <see cref="IQuerySafetyValidator"/>.
/// </summary>
internal sealed class TestQuerySafetyValidator : IQuerySafetyValidator
{
    private readonly Result _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestQuerySafetyValidator"/> class.
    /// </summary>
    /// <param name="result">The result to return.</param>
    public TestQuerySafetyValidator(Result result)
    {
        _result = result;
    }

    /// <summary>
    /// Gets the last query passed to <see cref="Validate"/>.
    /// </summary>
    public string? LastQuery { get; private set; }

    /// <summary>
    /// Returns the configured validation result.
    /// </summary>
    /// <param name="query">The query to validate.</param>
    /// <returns>The configured validation result.</returns>
    public Result Validate(string query)
    {
        LastQuery = query;
        return _result;
    }
}