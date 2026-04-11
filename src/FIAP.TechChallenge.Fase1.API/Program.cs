using FIAP.TechChallenge.Fase1.Application;
using FIAP.TechChallenge.Fase1.Infrastructure;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
}

if (!app.Environment.IsEnvironment("Testing"))
    await app.ApplyMigrationsAsync();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
