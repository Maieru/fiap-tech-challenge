using FIAP.TechChallenge.Fase1.Application;
using FIAP.TechChallenge.Fase1.Infrastructure;
using FIAP.TechChallenge.Fase1.Infrastructure.Observability;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicyName = "FrontendCors";

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        _ = policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Logging.ConfigureOpenTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
    _ = app.MapScalarApiReference();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.ApplyMigrationsAsync();
    _ = app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    _ = context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    _ = context.Response.Headers.TryAdd("Cross-Origin-Resource-Policy", "same-origin");

    await next();
});

app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapPrometheusScrapingEndpoint("/metrics");

await app.RunAsync();

