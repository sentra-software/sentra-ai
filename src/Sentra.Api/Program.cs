using Scalar.AspNetCore;
using Sentra.Api;
using Sentra.Application;
using Sentra.Connectors.PostgreSql;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSentraApi()
    .AddSentraApplication()
    .AddSentraPostgreSqlConnector();

WebApplication? app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Sentra API";
    });
}

app.MapControllers();

app.Run();

/// <summary>
/// Represents the application entry point type for the ASP.NET Core host.
/// </summary>
public partial class Program
{
}