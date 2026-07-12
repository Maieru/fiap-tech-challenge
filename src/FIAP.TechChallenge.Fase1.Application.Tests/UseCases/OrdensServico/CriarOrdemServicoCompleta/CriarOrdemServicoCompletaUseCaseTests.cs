using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoComClienteEVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoCompleta;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.CriarOrdemServicoCompleta;

[TestFixture]
internal sealed class CriarOrdemServicoCompletaUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldCreateLinksAndStartDiagnosis()
    {
        var ordemId = Guid.NewGuid();
        var ordem = new Mock<ICriarOrdemServicoComClienteEVeiculoUseCase>();
        var servico = new Mock<IAdicionarServicoOrdemServicoUseCase>();
        var peca = new Mock<IAdicionarPecaInsumoOrdemServicoUseCase>();
        var diagnostico = new Mock<IIniciarDiagnosticoOrdemServicoUseCase>();
        ordem.Setup(x => x.ExecuteAsync(It.IsAny<CriarOrdemServicoComClienteEVeiculoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CriarOrdemServicoResponse>.Success(new() { Id = ordemId }));
        servico.Setup(x => x.ExecuteAsync(It.IsAny<AdicionarServicoOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdicionarServicoOrdemServicoCommand x, CancellationToken _) => Result<AdicionarServicoOrdemServicoResponse>.Success(new() { OrdemServicoId = x.OrdemServicoId, ServicoId = x.ServicoId }));
        peca.Setup(x => x.ExecuteAsync(It.IsAny<AdicionarPecaInsumoOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdicionarPecaInsumoOrdemServicoCommand x, CancellationToken _) => Result<AdicionarPecaInsumoOrdemServicoResponse>.Success(new() { OrdemServicoId = x.OrdemServicoId, PecaInsumoId = x.PecaInsumoId }));
        diagnostico.Setup(x => x.ExecuteAsync(It.IsAny<IniciarDiagnosticoOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IniciarDiagnosticoOrdemServicoResponse>.Success(new() { Id = ordemId, Status = StatusOrdemServico.EmDiagnostico, DataInicioDiagnostico = DateTime.UtcNow }));
        var command = new CriarOrdemServicoCompletaCommand
        {
            Servicos = [new() { ServicoId = Guid.NewGuid(), Quantidade = 2 }],
            PecasInsumos = [new() { PecaInsumoId = Guid.NewGuid(), Quantidade = 3 }]
        };

        var result = await new CriarOrdemServicoCompletaUseCase(ordem.Object, servico.Object, peca.Object, diagnostico.Object).ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Status, Is.EqualTo(StatusOrdemServico.EmDiagnostico));
            Assert.That(result.Value.Servicos, Has.Count.EqualTo(1));
            Assert.That(result.Value.PecasInsumos, Has.Count.EqualTo(1));
        });
        diagnostico.Verify(x => x.ExecuteAsync(It.Is<IniciarDiagnosticoOrdemServicoCommand>(c => c.OrdemServicoId == ordemId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenLinkingAServiceFails()
    {
        var ordem = new Mock<ICriarOrdemServicoComClienteEVeiculoUseCase>();
        var servico = new Mock<IAdicionarServicoOrdemServicoUseCase>();
        var peca = new Mock<IAdicionarPecaInsumoOrdemServicoUseCase>();
        var diagnostico = new Mock<IIniciarDiagnosticoOrdemServicoUseCase>();
        var error = new Error("Servico nao encontrado.", ErrorCode.NotFound);
        ordem.Setup(x => x.ExecuteAsync(It.IsAny<CriarOrdemServicoComClienteEVeiculoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CriarOrdemServicoResponse>.Success(new() { Id = Guid.NewGuid() }));
        servico.Setup(x => x.ExecuteAsync(It.IsAny<AdicionarServicoOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AdicionarServicoOrdemServicoResponse>.Failure(error));
        diagnostico.Setup(x => x.ExecuteAsync(It.IsAny<IniciarDiagnosticoOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IniciarDiagnosticoOrdemServicoResponse>.Success(new() { Status = StatusOrdemServico.EmDiagnostico }));
        var command = new CriarOrdemServicoCompletaCommand { Servicos = [new() { ServicoId = Guid.NewGuid(), Quantidade = 1 }] };

        var result = await new CriarOrdemServicoCompletaUseCase(ordem.Object, servico.Object, peca.Object, diagnostico.Object).ExecuteAsync(command);

        Assert.That(result.Error, Is.EqualTo(error));
        peca.Verify(x => x.ExecuteAsync(It.IsAny<AdicionarPecaInsumoOrdemServicoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
