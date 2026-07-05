using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class HealthController(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<HealthController> logger) : ControllerBase
{
    private static readonly Guid _staticInstanceId = Guid.NewGuid();
    private static readonly ConfigurationVariable[] _requiredConfigurationVariables =
    [
        new("ASPNETCORE_ENVIRONMENT", "ASPNETCORE_ENVIRONMENT"),
        new("ASPNETCORE_URLS", "ASPNETCORE_URLS"),
        new("ConnectionStrings__DefaultConnection", "ConnectionStrings:DefaultConnection"),
        new("Jwt__SigningKey", "Jwt:SigningKey"),
        new("Jwt__Issuer", "Jwt:Issuer"),
        new("Jwt__Audience", "Jwt:Audience"),
        new("Jwt__AccessTokenMinutes", "Jwt:AccessTokenMinutes")
    ];

    [HttpGet("live")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult GetLiveness()
    {
        var response = new HealthResponse(HealthStatus.Healthy, _staticInstanceId, [], []);
        return Ok(response);
    }

    [HttpGet]
    [HttpGet("ready")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetReadiness(CancellationToken cancellationToken)
    {
        var database = await CheckDatabaseAsync(cancellationToken);
        var isHealthy = database.Status == HealthStatus.Healthy;
        var missingEnvironmentVariables = _requiredConfigurationVariables
            .Select(variable => new EnvironmentVariableStatus(
                variable.EnvironmentVariable,
                !string.IsNullOrWhiteSpace(configuration[variable.ConfigurationKey])))
            .Where(variable => !variable.Exists)
            .ToArray();

        var response = new HealthResponse(
            isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            _staticInstanceId,
            [database],
            missingEnvironmentVariables);

        return isHealthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    private async Task<DependencyStatus> CheckDatabaseAsync(CancellationToken cancellationToken)
    {
        var dbContext = serviceProvider.GetService<AppDbContext>();

        if (dbContext is null)
            return new DependencyStatus("database", HealthStatus.Unhealthy, "Database is not configured.");

        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return new DependencyStatus(
                "database",
                canConnect ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                canConnect ? null : "Database connection is unavailable.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Database readiness check failed.");
            return new DependencyStatus("database", HealthStatus.Unhealthy, "Database connection check failed.");
        }
    }

    public static class HealthStatus
    {
        public const string Healthy = "Healthy";
        public const string Unhealthy = "Unhealthy";
    }

    public sealed record HealthResponse(
        string Status,
        Guid InstanceId,
        IReadOnlyCollection<DependencyStatus> Dependencies,
        IReadOnlyCollection<EnvironmentVariableStatus> EnvironmentVariables);

    public sealed record DependencyStatus(string Name, string Status, string? Error);

    public sealed record EnvironmentVariableStatus(string Name, bool Exists);

    private sealed record ConfigurationVariable(string EnvironmentVariable, string ConfigurationKey);
}
