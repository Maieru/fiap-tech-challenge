using FIAP.TechChallenge.Fase1.Application;
using FIAP.TechChallenge.Fase1.Infrastructure;
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
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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

app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();
