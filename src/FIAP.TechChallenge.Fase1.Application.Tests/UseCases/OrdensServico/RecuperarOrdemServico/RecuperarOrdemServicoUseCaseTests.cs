using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.RecuperarOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.RecuperarOrdemServico;

[TestFixture]
internal sealed class RecuperarOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIdIsEmpty()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var pecaInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();

        var useCase = new RecuperarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
            pecaInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(new RecuperarOrdemServicoCommand { OrdemServicoId = Guid.Empty });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O identificador da ordem de servico deve ser valido."));
        });

        ordemServicoRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var pecaInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.")));

        var useCase = new RecuperarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
            pecaInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(new RecuperarOrdemServicoCommand { OrdemServicoId = Guid.NewGuid() });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
        });

        servicoDaOrdemRepositoryMock.Verify(x => x.GetByOrdemServicoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        pecaInsumoDaOrdemRepositoryMock.Verify(x => x.GetByOrdemServicoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenOrdemServicoExists()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var pecaInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();

        var ordemServico = CreateOrdemServico(StatusOrdemServico.EmDiagnostico);
        var servicoDaOrdem = CreateServicoDaOrdem(ordemServico.Id, "Alinhamento", 180m, 2);
        var pecaInsumoDaOrdem = CreatePecaInsumoDaOrdem(ordemServico.Id, "Filtro de ar", "FLT-001", 49m, 3);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Success(new[] { servicoDaOrdem }));

        _ = pecaInsumoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>>.Success(new[] { pecaInsumoDaOrdem }));

        var useCase = new RecuperarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
            pecaInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(new RecuperarOrdemServicoCommand { OrdemServicoId = ordemServico.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(ordemServico.Id));
            Assert.That(result.Value.ClienteId, Is.EqualTo(ordemServico.ClienteId));
            Assert.That(result.Value.VeiculoId, Is.EqualTo(ordemServico.VeiculoId));
            Assert.That(result.Value.Status, Is.EqualTo(StatusOrdemServico.EmDiagnostico));
            Assert.That(result.Value.Servicos, Has.Count.EqualTo(1));
            Assert.That(result.Value.PecasInsumos, Has.Count.EqualTo(1));
            Assert.That(result.Value.ValorTotalServicos, Is.EqualTo(360m));
            Assert.That(result.Value.ValorTotalPecasInsumos, Is.EqualTo(147m));
            Assert.That(result.Value.ValorTotalOrdemServico, Is.EqualTo(507m));
        });
    }

    private static OrdemServico CreateOrdemServico(StatusOrdemServico status)
    {
        var ordemServicoResult = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Ruido no sistema de direcao");

        Assert.Multiple(() =>
        {
            Assert.That(ordemServicoResult.IsSuccess, Is.True);
            Assert.That(ordemServicoResult.Value, Is.Not.Null);
        });

        var ordemServico = ordemServicoResult.Value!;

        if (status == StatusOrdemServico.EmDiagnostico)
        {
            var iniciarDiagnosticoResult = ordemServico.IniciarDiagnostico();

            Assert.Multiple(() =>
            {
                Assert.That(iniciarDiagnosticoResult.IsSuccess, Is.True);
                Assert.That(iniciarDiagnosticoResult.Value, Is.True);
            });
        }

        return ordemServico;
    }

    private static ServicoDaOrdemDeServico CreateServicoDaOrdem(Guid ordemServicoId, string descricao, decimal valorUnitario, int quantidade)
    {
        var servicoResult = Servico.Create(descricao, valorUnitario);

        Assert.Multiple(() =>
        {
            Assert.That(servicoResult.IsSuccess, Is.True);
            Assert.That(servicoResult.Value, Is.Not.Null);
        });

        var servicoDaOrdemResult = ServicoDaOrdemDeServico.Create(ordemServicoId, servicoResult.Value!, quantidade);

        Assert.Multiple(() =>
        {
            Assert.That(servicoDaOrdemResult.IsSuccess, Is.True);
            Assert.That(servicoDaOrdemResult.Value, Is.Not.Null);
        });

        return servicoDaOrdemResult.Value!;
    }

    private static PecaOuInsumoDaOrdemDeServico CreatePecaInsumoDaOrdem(Guid ordemServicoId, string nome, string codigo, decimal precoUnitario, int quantidade)
    {
        var pecaInsumoResult = PecaInsumo.Create(nome, codigo, "Descricao da peca", precoUnitario, 20);

        Assert.Multiple(() =>
        {
            Assert.That(pecaInsumoResult.IsSuccess, Is.True);
            Assert.That(pecaInsumoResult.Value, Is.Not.Null);
        });

        var pecaInsumoDaOrdemResult = PecaOuInsumoDaOrdemDeServico.Create(ordemServicoId, pecaInsumoResult.Value!, quantidade);

        Assert.Multiple(() =>
        {
            Assert.That(pecaInsumoDaOrdemResult.IsSuccess, Is.True);
            Assert.That(pecaInsumoDaOrdemResult.Value, Is.Not.Null);
        });

        return pecaInsumoDaOrdemResult.Value!;
    }
}

