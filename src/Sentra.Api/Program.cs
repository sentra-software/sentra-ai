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
using Sentra.Connectors.PostgreSql;
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
    .AddSentraPostgreSqlConnector();

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

using(IServiceScope scope = app.Services.CreateScope())
{
    SentraPlatformDbContext dbContext = scope.ServiceProvider.GetRequiredService<SentraPlatformDbContext>();
    dbContext.Database.Migrate();
}

app.Run();

/// <summary>
/// Represents the application entry point type for the ASP.NET Core host.
/// </summary>
public partial class Program
{
}