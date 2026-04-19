using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.AtualizarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ListarClientes;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.RecuperarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.EntradaEstoquePecaInsumo;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.AtualizarPecaInsumo;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.IncluirPecaInsumo;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.ListarPecasInsumos;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.RecuperarPecaInsumo;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ListarOrdensServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.RecuperarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.SolicitarAprovacaoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.AtualizarVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.CriarVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ListarVeiculos;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.RecuperarVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.CadastrarServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.AtualizarServico;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.TechChallenge.Fase1.Application;

public static class ApplicationDependecyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        _ = services.AddScoped<ICriarClienteUseCase, CriarClienteUseCase>();
        _ = services.AddScoped<IAtualizarClienteUseCase, AtualizarClienteUseCase>();
        _ = services.AddScoped<IListarClientesUseCase, ListarClientesUseCase>();
        _ = services.AddScoped<IRecuperarClienteUseCase, RecuperarClienteUseCase>();
        _ = services.AddScoped<ICriarVeiculoUseCase, CriarVeiculoUseCase>();
        _ = services.AddScoped<IAtualizarVeiculoUseCase, AtualizarVeiculoUseCase>();
        _ = services.AddScoped<IListarVeiculosUseCase, ListarVeiculosUseCase>();
        _ = services.AddScoped<IRecuperarVeiculoUseCase, RecuperarVeiculoUseCase>();
        _ = services.AddScoped<ICriarOrdemServicoUseCase, CriarOrdemServicoUseCase>();
        _ = services.AddScoped<IAdicionarPecaInsumoOrdemServicoUseCase, AdicionarPecaInsumoOrdemServicoUseCase>();
        _ = services.AddScoped<IAdicionarServicoOrdemServicoUseCase, AdicionarServicoOrdemServicoUseCase>();
        _ = services.AddScoped<IIniciarDiagnosticoOrdemServicoUseCase, IniciarDiagnosticoOrdemServicoUseCase>();
        _ = services.AddScoped<ISolicitarAprovacaoOrdemServicoUseCase, SolicitarAprovacaoOrdemServicoUseCase>();
        _ = services.AddScoped<IListarOrdensServicoUseCase, ListarOrdensServicoUseCase>();
        _ = services.AddScoped<IRecuperarOrdemServicoUseCase, RecuperarOrdemServicoUseCase>();
        _ = services.AddScoped<IIncluirPecaInsumoUseCase, IncluirPecaInsumoUseCase>();
        _ = services.AddScoped<IEntradaEstoquePecaInsumoUseCase, EntradaEstoquePecaInsumoUseCase>();
        _ = services.AddScoped<IAtualizarPecaInsumoUseCase, AtualizarPecaInsumoUseCase>();
        _ = services.AddScoped<IListarPecasInsumosUseCase, ListarPecasInsumosUseCase>();
        _ = services.AddScoped<IRecuperarPecaInsumoUseCase, RecuperarPecaInsumoUseCase>();
        _ = services.AddScoped<ICadastrarServicoUseCase, CadastrarServicoUseCase>();
        _ = services.AddScoped<IAtualizarServicoUseCase, AtualizarServicoUseCase>();
        return services;
    }
}
