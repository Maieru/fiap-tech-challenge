using FIAP.TechChallenge.OS.Application;
using FIAP.TechChallenge.OS.API.Security;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Infrastructure;
using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using Maieru.FiapTechChallenge.Chassis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using Scalar.AspNetCore;

namespace FIAP.TechChallenge.OS.API;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        _ = builder.Services.AddControllers();
        _ = builder.Services.AddOpenApi();
        _ = builder.Services.AddApplication();
        _ = builder.Services.AddInfrastructure(builder.Configuration);
        _ = builder.AddChassis(new ChassisOptions
        {
            ServiceName = "oficina-os",
            EnableOtlp = builder.Configuration.GetValue<bool>("Observability:EnableOtlp"),
            MeterNames = ["Fiap.TechChallenge.OS"]
        });
        _ = builder.Services.AddJwtAuthentication();
        _ = builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("postgresql", tags: ["ready"]);
        _ = builder.Services.AddHealthChecks().AddCheck<KafkaReadiness>("kafka", tags: ["ready"]);
        var app = builder.Build();
        _ = app.UseChassis();
        _ = app.Use(async (context, next) =>
        {
            var messaging = context.RequestServices.GetRequiredService<MessageContext>();
            messaging.CorrelationId = context.Items[ChassisExtensions.CorrelationHeader]?.ToString() ?? messaging.CorrelationId;
            try
            { await next(context); }
            catch (ConcurrencyException)
            {
                await Results.Problem(statusCode: 409, title: "A ordem foi alterada por outra operação. Consulte novamente.").ExecuteAsync(context);
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                await Results.Problem(statusCode: 409, title: "Já existe um cadastro ativo com estes dados.").ExecuteAsync(context);
            }
            catch (DbUpdateConcurrencyException)
            {
                await Results.Problem(statusCode: 404, title: "Cadastro não encontrado ou inativo.").ExecuteAsync(context);
            }
            catch (KeyNotFoundException)
            {
                await Results.Problem(statusCode: 404, title: "Cadastro não encontrado ou inativo.").ExecuteAsync(context);
            }
        });
        _ = app.UseAuthentication();
        _ = app.UseAuthorization();
        _ = app.MapControllers();
        _ = app.MapChassisHealthChecks("/api/os/v1/health");
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            _ = app.MapOpenApi();
            _ = app.MapScalarApiReference();
        }
        if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
        {
            await using var scope = app.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        }
        await app.RunAsync();
    }

}

public sealed class DatabaseHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return await db.Database.CanConnectAsync(cancellationToken)
                && !(await db.Database.GetPendingMigrationsAsync(cancellationToken)).Any()
                ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}
