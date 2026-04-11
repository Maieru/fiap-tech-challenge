using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.AtualizarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ListarClientes;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.AtualizarVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.CriarVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ListarVeiculos;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.TechChallenge.Fase1.Application;

public static class ApplicationDependecyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        _ = services.AddScoped<ICriarClienteUseCase, CriarClienteUseCase>();
        _ = services.AddScoped<IAtualizarClienteUseCase, AtualizarClienteUseCase>();
        _ = services.AddScoped<IListarClientesUseCase, ListarClientesUseCase>();
        _ = services.AddScoped<ICriarVeiculoUseCase, CriarVeiculoUseCase>();
        _ = services.AddScoped<IAtualizarVeiculoUseCase, AtualizarVeiculoUseCase>();
        _ = services.AddScoped<IListarVeiculosUseCase, ListarVeiculosUseCase>();
        return services;
    }
}
