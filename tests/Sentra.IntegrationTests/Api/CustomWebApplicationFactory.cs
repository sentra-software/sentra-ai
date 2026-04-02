using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

using Sentra.Application.Abstractions.DataSources;

namespace Sentra.IntegrationTests.Api;

/// <summary>
/// Provides a configurable web application factory for API integration tests.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Mock<IDataSourceConnectionService>? _connectionServiceMock;
    private readonly Mock<IDataSourceSchemaService>? _schemaServiceMock;
    private readonly Mock<IDataSourceQueryService>? _queryServiceMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomWebApplicationFactory"/> class.
    /// </summary>
    public CustomWebApplicationFactory(
        Mock<IDataSourceConnectionService>? connectionServiceMock = null,
        Mock<IDataSourceSchemaService>? schemaServiceMock = null,
        Mock<IDataSourceQueryService>? queryServiceMock = null)
    {
        _connectionServiceMock = connectionServiceMock;
        _schemaServiceMock = schemaServiceMock;
        _queryServiceMock = queryServiceMock;

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__SentraPlatform",
            "Host=localhost;Port=5432;Database=sentra_test;Username=test;Password=test");

        Environment.SetEnvironmentVariable("Jwt__Issuer", "sentra-test");
        Environment.SetEnvironmentVariable("Jwt__Audience", "sentra-test-audience");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "super-secure-test-signing-key-123456789");
        Environment.SetEnvironmentVariable("Jwt__ExpirationInMinutes", "60");
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            if (_connectionServiceMock is not null)
            {
                services.RemoveAll<IDataSourceConnectionService>();
                services.AddSingleton(_ => _connectionServiceMock.Object);
            }

            if (_schemaServiceMock is not null)
            {
                services.RemoveAll<IDataSourceSchemaService>();
                services.AddSingleton(_ => _schemaServiceMock.Object);
            }

            if (_queryServiceMock is not null)
            {
                services.RemoveAll<IDataSourceQueryService>();
                services.AddSingleton(_ => _queryServiceMock.Object);
            }
        });
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__SentraPlatform", null);
        Environment.SetEnvironmentVariable("Jwt__Issuer", null);
        Environment.SetEnvironmentVariable("Jwt__Audience", null);
        Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
        Environment.SetEnvironmentVariable("Jwt__ExpirationInMinutes", null);

        await base.DisposeAsync();
    }
}