using FluentAssertions;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.UnitTests.Connectors.Abstractions.Schema;

/// <summary>
/// Contains unit tests for <see cref="TableSchema"/> and <see cref="ColumnSchema"/>.
/// </summary>
public sealed class TableSchemaTests
{
    /// <summary>
    /// Verifies that a table schema preserves the provided values.
    /// </summary>
    [Fact]
    public void Constructor_Should_Assign_Values()
    {
        ColumnSchema[]? columns = new[]
        {
            new ColumnSchema("id", "uuid", false),
            new ColumnSchema("name", "text", true)
        };

        TableSchema? tableSchema = new TableSchema("public", "customers", columns);

        tableSchema.Schema.Should().Be("public");
        tableSchema.Name.Should().Be("customers");
        tableSchema.Columns.Should().HaveCount(2);
        tableSchema.Columns.First().Name.Should().Be("id");
    }
}