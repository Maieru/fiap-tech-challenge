using Confluent.Kafka;
using FIAP.TechChallenge.OS.Infrastructure.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FIAP.TechChallenge.OS.API;

public sealed class KafkaReadiness(IConfiguration configuration, ConsumerState state) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!state.Running)
                return Task.FromResult(HealthCheckResult.Unhealthy("Consumidor não está ativo."));

            using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = configuration["Kafka:BootstrapServers"] }).Build();

            return Task.FromResult(admin.GetMetadata(TimeSpan.FromSeconds(3)).Brokers.Count > 0
                ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
        }
        catch (Exception exception) when (exception is not OperationCanceledException) { return Task.FromResult(HealthCheckResult.Unhealthy()); }
    }
}
