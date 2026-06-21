using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class HealthController(IServiceProvider serviceProvider) : ControllerBase
{
    private static readonly Guid StaticInstanceId = Guid.NewGuid();

    private static readonly string[] RequiredEnvironmentVariables =
    [
        "ASPNETCORE_ENVIRONMENT",
        "ASPNETCORE_URLS",
        "ConnectionStrings__DefaultConnection",
        "Jwt__SigningKey",
        "Jwt__Issuer",
        "Jwt__Audience",
        "Jwt__AccessTokenMinutes"
    ];

    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var database = await CheckDatabaseAsync(cancellationToken);
        var missingEnvironmentVariables = RequiredEnvironmentVariables
            .Select(variable => new EnvironmentVariableStatus(
                variable,
                !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variable))))
            .Where(variable => !variable.Exists)
            .ToArray();

        var isHealthy = database.Status == HealthStatus.Healthy
            && missingEnvironmentVariables.Length == 0;

        var response = new HealthResponse(
            isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            StaticInstanceId,
            [database],
            missingEnvironmentVariables);

        return Ok(response);
    }

    private async Task<DependencyStatus> CheckDatabaseAsync(CancellationToken cancellationToken)
    {
        var dbContext = serviceProvider.GetService<AppDbContext>();

        if (dbContext is null)
            return new DependencyStatus("database", HealthStatus.Unhealthy, "AppDbContext is not configured.");

        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return new DependencyStatus(
                "database",
                canConnect ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                canConnect ? null : "Database connection is unavailable.");
        }
        catch (Exception exception)
        {
            return new DependencyStatus("database", HealthStatus.Unhealthy, exception.Message);
        }
    }

    public static class HealthStatus
    {
        public const string Healthy = "Healthy";
        public const string Unhealthy = "Unhealthy";
    }

    public sealed record HealthResponse(string Status, Guid InstanceId, IReadOnlyCollection<DependencyStatus> Dependencies, IReadOnlyCollection<EnvironmentVariableStatus> EnvironmentVariables);

    public sealed record DependencyStatus(string Name, string Status, string? Error);

    public sealed record EnvironmentVariableStatus(string Name, bool Exists);
}
