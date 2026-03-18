using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Sentra.AI.Orchestration;
using Sentra.Api;
using Sentra.Application;
using Sentra.Connectors.PostgreSql;
using Sentra.Infrastructure;
using Sentra.Security;
using Sentra.Security.Jwt;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Sentra API";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// Represents the application entry point type for the ASP.NET Core host.
/// </summary>
public partial class Program
{
}