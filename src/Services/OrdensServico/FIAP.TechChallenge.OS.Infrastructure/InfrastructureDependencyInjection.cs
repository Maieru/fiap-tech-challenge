using FIAP.TechChallenge.OS.Domain.Interfaces;
using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using FIAP.TechChallenge.OS.Infrastructure.Persistence.Repositories;
using FIAP.TechChallenge.OS.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.TechChallenge.OS.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>((provider, options) => options.UseNpgsql(
            provider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection deve ser configurada.")));
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<OrdemServicoRepository>();
        services.AddScoped<IOrdemServicoRepository>(provider => provider.GetRequiredService<OrdemServicoRepository>());
        services.AddScoped<IHistoricoOrdemRepository>(provider => provider.GetRequiredService<OrdemServicoRepository>());
        services.AddScoped<IntegrationContext>();
        services.AddScoped<FluxoOrdemRepository>();
        services.AddScoped<IFluxoOrdemRepository>(provider => provider.GetRequiredService<FluxoOrdemRepository>());
        services.AddScoped<IIntegrationEventHandler>(provider => provider.GetRequiredService<FluxoOrdemRepository>());
        services.AddScoped<IOutboxStore, PostgresOutbox>();
        services.AddSingleton(provider =>
        {
            var messaging = provider.GetRequiredService<IConfiguration>().GetSection("Messaging").Get<KafkaOptions>() ?? new();
            messaging.RetryDelaysSeconds = provider.GetRequiredService<IConfiguration>().GetSection("Messaging:RetryDelaysSeconds").Get<int[]>() ?? [1, 2, 4, 8, 16];
            messaging.InputTopic = "fiap.execution.events.v1"; messaging.OutputTopic = "fiap.os.events.v1"; messaging.GroupId = "fiap-os-v1";
            return messaging;
        });
        services.AddHostedService<KafkaWorker>(); services.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka");
        return services;
    }
}
