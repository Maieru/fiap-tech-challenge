using FIAP.TechChallenge.OS.Application;
using FIAP.TechChallenge.OS.Infrastructure;
using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using FIAP.TechChallenge.OS.Infrastructure.Messaging;
using Maieru.FiapTechChallenge.Chassis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Scalar.AspNetCore;
using System.Text;

namespace FIAP.TechChallenge.OS.API;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.AddChassis(new ChassisOptions
        {
            ServiceName = "oficina-os",
            EnableOtlp = builder.Configuration.GetValue<bool>("Observability:EnableOtlp"),
            MeterNames = ["Fiap.TechChallenge.OS", KafkaWorker.InstrumentationName], ActivitySourceNames = [KafkaWorker.InstrumentationName]
        });
        ConfigureAuthentication(builder);
        builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("postgresql", tags: ["ready"]);
        var app = builder.Build();
        app.UseChassis();
        app.Use(async (context, next) =>
        {
            try { await next(context); }
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
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapChassisHealthChecks("/api/os/v1/health");
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
        {
            await using var scope = app.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        }
        await app.RunAsync();
    }

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme).Configure<IConfiguration>((options, configuration) =>
        {
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];
            var key = Environment.GetEnvironmentVariable("Jwt__SigningKey");
            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
                throw new InvalidOperationException("Configure Jwt:Issuer, Jwt:Audience e Jwt:SigningKey (mínimo de 32 bytes). O OS apenas valida tokens.");
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = issuer, ValidateAudience = true, ValidAudience = audience,
                ValidateLifetime = true, ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), ClockSkew = TimeSpan.Zero
            };
            options.MapInboundClaims = false;
        }).ValidateOnStart();
        builder.Services.AddAuthorization();
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
