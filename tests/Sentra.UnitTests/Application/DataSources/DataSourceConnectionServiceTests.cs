using FluentAssertions;
using Sentra.Application.Connectors;
using Sentra.Application.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Application.DataSources;

/// <summary>
/// Contains unit tests for <see cref="DataSourceConnectionService"/>.
/// </summary>
public sealed class DataSourceConnectionServiceTests
{
    /// <summary>
    /// Verifies that a PostgreSQL data source uses the PostgreSQL connector.
    /// </summary>
    [Fact]
    public async Task TestConnectionAsync_Should_Use_PostgreSql_Connector_For_PostgreSql_DataSource()
    {
        TestDataConnector? connector = new TestDataConnector(
            ConnectorType.PostgreSql,
            ConnectionTestResult.Success("Connection succeeded."));

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        DataSourceConnectionService? service = new DataSourceConnectionService(registry);

        Result<ConnectionTestResult>? result = await service.TestConnectionAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;Database=sentra;");

        result.IsSuccess.Should().BeTrue();
        result.ValueOrThrow().IsSuccess.Should().BeTrue();
        result.ValueOrThrow().Message.Should().Be("Connection succeeded.");
        connector.LastConnectionString.Should().Be("Host=localhost;Database=sentra;");
    }

    /// <summary>
    /// Verifies that the connection string is trimmed before the connector is called.
    /// </summary>
    [Fact]
    public async Task TestConnectionAsync_Should_Trim_Connection_String()
    {
        TestDataConnector? connector = new TestDataConnector(
            ConnectorType.PostgreSql,
            ConnectionTestResult.Success("OK"));

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        DataSourceConnectionService? service = new DataSourceConnectionService(registry);

        Result<ConnectionTestResult>? result = await service.TestConnectionAsync(
            DataSourceType.PostgreSql,
            "  Host=localhost;Database=sentra;  ");

        result.IsSuccess.Should().BeTrue();
        connector.LastConnectionString.Should().Be("Host=localhost;Database=sentra;");
    }

    /// <summary>
    /// Verifies that an empty connection string is rejected.
    /// </summary>
    [Fact]
    public async Task TestConnectionAsync_Should_Return_Failure_When_Connection_String_Is_Empty()
    {
        TestDataConnector? connector = new TestDataConnector(
            ConnectorType.PostgreSql,
            ConnectionTestResult.Success("OK"));

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        DataSourceConnectionService? service = new DataSourceConnectionService(registry);

        Result<ConnectionTestResult>? result = await service.TestConnectionAsync(
            DataSourceType.PostgreSql,
            " ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.connection_string.required");
    }

    /// <summary>
    /// Verifies that an invalid data source type is rejected.
    /// </summary>
    [Fact]
    public async Task TestConnectionAsync_Should_Return_Failure_When_DataSourceType_Is_Invalid()
    {
        TestDataConnector? connector = new TestDataConnector(
            ConnectorType.PostgreSql,
            ConnectionTestResult.Success("OK"));

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        DataSourceConnectionService? service = new DataSourceConnectionService(registry);

        Result<ConnectionTestResult>? result = await service.TestConnectionAsync(
            (DataSourceType)999,
            "Host=localhost;");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasources.type.invalid");
    }

    /// <summary>
    /// Verifies that a missing connector registration returns a failure.
    /// </summary>
    [Fact]
    public async Task TestConnectionAsync_Should_Return_Failure_When_Connector_Is_Not_Registered()
    {
        ConnectorRegistry? registry = new ConnectorRegistry(
        [
            new TestDataConnector(
                ConnectorType.PostgreSql,
                ConnectionTestResult.Success("OK"))
        ]);

        DataSourceConnectionService? service = new DataSourceConnectionService(registry);

        Result<ConnectionTestResult>? result = await service.TestConnectionAsync(
            DataSourceType.MySql,
            "Server=localhost;");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("connectors.not_registered");
    }

    /// <summary>
    /// Verifies that a failed connector connection test is still returned successfully as an executed test result.
    /// </summary>
    [Fact]
    public async Task TestConnectionAsync_Should_Return_Connector_Result_When_Test_Fails()
    {
        TestDataConnector? connector = new TestDataConnector(
            ConnectorType.PostgreSql,
            ConnectionTestResult.Failure("Connection failed."));

        ConnectorRegistry? registry = new ConnectorRegistry([connector]);
        DataSourceConnectionService? service = new DataSourceConnectionService(registry);

        Result<ConnectionTestResult>? result = await service.TestConnectionAsync(
            DataSourceType.PostgreSql,
            "Host=localhost;");

        result.IsSuccess.Should().BeTrue();
        result.ValueOrThrow().IsSuccess.Should().BeFalse();
        result.ValueOrThrow().Message.Should().Be("Connection failed.");
    }

    /// <summary>
    /// Verifies that the constructor rejects a null connector registry.
    /// </summary>
    [Fact]
    public void Constructor_Should_Throw_When_ConnectorRegistry_Is_Null()
    {
        Func<DataSourceConnectionService>? action = () => new DataSourceConnectionService(null!);

        action.Should().Throw<ArgumentNullException>();
    }
}