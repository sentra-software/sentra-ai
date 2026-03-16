using FluentAssertions;
using Sentra.Connectors.Abstractions.Querying;

namespace Sentra.UnitTests.Connectors.Abstractions.Querying;

/// <summary>
/// Contains unit tests for <see cref="QueryExecutionResult"/>.
/// </summary>
public sealed class QueryExecutionResultTests
{
    /// <summary>
    /// Verifies that a query execution result preserves its provided values.
    /// </summary>
    [Fact]
    public void Constructor_Should_Assign_Values()
    {
        string[]? columns = new[] { "id", "name" };

        Dictionary<string, object?>[]? rows = new[]
        {
            new Dictionary<string, object?>
            {
                ["id"] = 1,
                ["name"] = "Josey"
            }
        };

        QueryExecutionResult? result = new QueryExecutionResult(columns, rows, 1);

        result.Columns.Should().ContainInOrder("id", "name");
        result.Rows.Should().HaveCount(1);
        result.RowCount.Should().Be(1);
    }
}