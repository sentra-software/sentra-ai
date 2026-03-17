using FluentAssertions;
using Sentra.Application.Connectors;
using Sentra.Application.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Contains unit tests for <see cref="DataSourceSchemaService"/>.
/// </summary>
public sealed class DataSourceSchemaServiceTests
{
    /// <summary>
    /// Verifies that a PostgreSQL data source uses the PostgreSQL connector and returns schema.
    /// </summary>
    [Fact]
    public async Task ReadSchemaAsync_Should_Use_PostgreSql_Connector_For_PostgreSql_DataSource()
    {
        IReadOnlyCollection<TableSchema> schema =
        [
            new TableSchema(
                "public",
                "customers",
                new[]
                {
                    new ColumnSchema("id", "uuid", false),
                    new ColumnSchema("name", "text", true)
                })
        ];

        TestSchemaDataConnector? connector = new TestSchemaDataConnector(
            ConnectorType.PostgreSql,
            schema);

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        DataSourceSchemaService? service = new DataSourceSchemaService(registry);

        Result<IReadOnlyCollection<TableSchema>>? result = await service.ReadSchemaAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;");

        result.IsSuccess.Should().BeTrue();
        result.ValueOrThrow().Should().HaveCount(1);
        result.ValueOrThrow().First().Name.Should().Be("customers");
        connector.LastConnectionString.Should().Be("Host=localhost;Database=sentra;");
    }

    /// <summary>
    /// Verifies that the connection string is trimmed before schema discovery runs.
    /// </summary>
    [Fact]
    public async Task ReadSchemaAsync_Should_Trim_Connection_String()
    {
        IReadOnlyCollection<TableSchema> schema = Array.Empty<TableSchema>();

        TestSchemaDataConnector? connector = new TestSchemaDataConnector(
            ConnectorType.PostgreSql,
            schema);

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        DataSourceSchemaService? service = new DataSourceSchemaService(registry);

        Result<IReadOnlyCollection<TableSchema>>? result = await service.ReadSchemaAsync(
            DataSourceType.PostgreSql,
            "  Host=localhost;Database=sentra;  ");

        result.IsSuccess.Should().BeTrue();
        connector.LastConnectionString.Should().Be("Host=localhost;Database=sentra;");
    }

    /// <summary>
    /// Verifies that an empty connection string is rejected.
    /// </summary>
    [Fact]
    public async Task ReadSchemaAsync_Should_Return_Failure_When_Connection_String_Is_Empty()
    {
        TestSchemaDataConnector? connector = new TestSchemaDataConnector(
            ConnectorType.PostgreSql,
            Array.Empty<TableSchema>());

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        DataSourceSchemaService? service = new DataSourceSchemaService(registry);

        Result<IReadOnlyCollection<TableSchema>>? result = await service.ReadSchemaAsync(
            DataSourceType.PostgreSql,
            " ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.connection_string.required");
    }

    /// <summary>
    /// Verifies that an invalid data source type is rejected.
    /// </summary>
    [Fact]
    public async Task ReadSchemaAsync_Should_Return_Failure_When_DataSourceType_Is_Invalid()
    {
        TestSchemaDataConnector? connector = new TestSchemaDataConnector(
            ConnectorType.PostgreSql,
            Array.Empty<TableSchema>());

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        DataSourceSchemaService? service = new DataSourceSchemaService(registry);

        Result<IReadOnlyCollection<TableSchema>>? result = await service.ReadSchemaAsync(
            (DataSourceType)999,
            "Host=localhost;");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.type.invalid");
    }

    /// <summary>
    /// Verifies that a missing connector registration returns a failure.
    /// </summary>
    [Fact]
    public async Task ReadSchemaAsync_Should_Return_Failure_When_Connector_Is_Not_Registered()
    {
        ConnectorRegistry? registry = new ConnectorRegistry(
        [
            new TestSchemaDataConnector(
                ConnectorType.PostgreSql,
                Array.Empty<TableSchema>())
        ]);

        DataSourceSchemaService? service = new DataSourceSchemaService(registry);

        Result<IReadOnlyCollection<TableSchema>>? result = await service.ReadSchemaAsync(
            DataSourceType.MySql,
            "Server=localhost;");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("connectors.not_registered");
    }

    /// <summary>
    /// Verifies that the constructor rejects a null connector registry.
    /// </summary>
    [Fact]
    public void Constructor_Should_Throw_When_ConnectorRegistry_Is_Null()
    {
        Func<DataSourceSchemaService>? action = () => new DataSourceSchemaService(null!);

        action.Should().Throw<ArgumentNullException>();
    }
}