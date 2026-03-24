using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Sentra.AI.Orchestration;
using Sentra.Api;
using Sentra.Application;
using Sentra.Connectors.MySql;
using Sentra.Connectors.PostgreSql;
using Sentra.Connectors.Sqlite;
using Sentra.Connectors.SqlServer;
using Sentra.Infrastructure;
using Sentra.Infrastructure.Persistence;
using Sentra.Security;
using Sentra.Security.Extensions;
using Sentra.Security.Jwt;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string publicApiBaseUrl =
    builder.Configuration["App:PublicBaseUrl"]
    ?? "http://api.sentra-software.nl";

builder.Services
    .AddSentraApi()
    .AddSentraApplication()
    .AddSentraAiOrchestration(builder.Configuration)
    .AddSentraPostgreSqlConnector()
    .AddSentraSqlServerConnector()
    .AddSentraMySqlConnector()
    .AddSentraSqliteConnector();

builder.Services.AddSentraInfrastructure(builder.Configuration);
builder.Services.AddSentraSecurity(builder.Configuration);

JwtOptions jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT settings are not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageDataSources", policy =>
        policy.RequireClaim("platform_role", "Owner", "Admin"));

    options.AddPolicy("CanViewAuditLogs", policy =>
        policy.RequireClaim("platform_role", "Owner", "Admin"));

    options.AddPolicy("CanUseChat", policy =>
        policy.RequireClaim("platform_role", "Owner", "Admin", "Member"));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
    {
        string partitionKey =
            context.Connection.RemoteIpAddress?.ToString() ??
            "unknown-ip";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.AddPolicy("ai-chat", context =>
    {
        Guid userId = context.User.GetIdentityUserId();
        string partitionKey = userId == Guid.Empty
            ? $"anon:{context.Connection.RemoteIpAddress}"
            : $"user:{userId}";

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey,
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 20,
                TokensPerPeriod = 20,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.AddPolicy("audit-api", context =>
    {
        Guid userId = context.User.GetIdentityUserId();
        string partitionKey = userId == Guid.Empty
            ? $"anon:{context.Connection.RemoteIpAddress}"
            : $"user:{userId}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.AddPolicy("managed-data-sources", context =>
    {
        Guid userId = context.User.GetIdentityUserId();
        string partitionKey = userId == Guid.Empty
            ? $"anon:{context.Connection.RemoteIpAddress}"
            : $"user:{userId}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "Sentra API";
        options.Servers = [new ScalarServer(publicApiBaseUrl)];
    });
}

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        SentraPlatformDbContext dbContext = services.GetRequiredService<SentraPlatformDbContext>();

        string connectionString = dbContext.Database.GetConnectionString() ?? "<null>";
        logger.LogInformation("Applying migrations for SentraPlatformDbContext...");
        logger.LogInformation("Connection string: {ConnectionString}", connectionString);

        IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        logger.LogInformation("Pending migrations count: {Count}", pendingMigrations.Count());

        foreach (string migration in pendingMigrations)
        {
            logger.LogInformation("Pending migration: {Migration}", migration);
        }

        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

app.Run();

/// <summary>
/// Represents the application entry point type for the ASP.NET Core host.
/// </summary>
public partial class Program
{
}