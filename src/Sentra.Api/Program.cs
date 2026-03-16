WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

WebApplication? app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    name = "Sentra AI",
    status = "running",
    environment = app.Environment.EnvironmentName,
    utc = DateTime.UtcNow
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Sentra.Api"
}));

app.Run();