using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.TechChallenge.Fase1.Infrastructure;

public static class InfraestructureDependecyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrWhiteSpace(connectionString))
            _ = services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        _ = services.AddScoped<IClienteRepository, ClienteRepository>();
        _ = services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        _ = services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
        _ = services.AddScoped<IPecaInsumoRepository, PecaInsumoRepository>();

        return services;
    }
}
