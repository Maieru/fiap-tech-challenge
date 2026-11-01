using FIAP.TechChallenge.OS.Domain.Interfaces;
using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using FIAP.TechChallenge.OS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Confluent.Kafka;
using FIAP.TechChallenge.OS.Application.Abstractions;
using FIAP.TechChallenge.OS.Infrastructure.Messaging;
using Maieru.FiapTechChallenge.Chassis;
using Maieru.FiapTechChallenge.Messaging.Kafka;
using Microsoft.Extensions.Hosting;

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
        services.AddScoped<IUnitOfWork, PostgresUnitOfWork>();
        services.AddScoped<IProcessedMessageRepository, ProcessedMessageRepository>();
        services.AddSingleton<IProducer<string, string>>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"] ?? throw new InvalidOperationException("Configure Kafka:BootstrapServers."),
                ClientId = "oficina-os", EnableIdempotence = true, Acks = Acks.All,
                MessageTimeoutMs = config.GetValue<int?>("Kafka:MessageTimeoutMs") ?? 10_000
            }).Build();
        });
        services.AddSingleton<IMessagingService, KafkaMessagingService>();
        services.AddSingleton<ConsumerState>();
        services.Configure<HostOptions>(options => options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost);
        services.AddHostedService<OsKafkaConsumer>();
        return services;
    }
}
