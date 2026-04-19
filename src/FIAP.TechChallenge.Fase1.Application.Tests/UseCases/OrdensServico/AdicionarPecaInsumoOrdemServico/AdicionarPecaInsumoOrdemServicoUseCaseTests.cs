using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;

[TestFixture]
internal sealed class AdicionarPecaInsumoOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var pecaInsumoRepositoryMock = new Mock<IPecaInsumoRepository>();
        var pecaOuInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.")));

        var useCase = new AdicionarPecaInsumoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            pecaInsumoRepositoryMock.Object,
            pecaOuInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
        });

        pecaInsumoRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        pecaOuInsumoDaOrdemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PecaOuInsumoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
        pecaInsumoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenPecaInsumoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var pecaInsumoRepositoryMock = new Mock<IPecaInsumoRepository>();
        var pecaOuInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = pecaInsumoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PecaInsumo>.Failure(new Error("Peca ou insumo nao encontrado.")));

        var useCase = new AdicionarPecaInsumoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            pecaInsumoRepositoryMock.Object,
            pecaOuInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServicoId: ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Peca ou insumo nao encontrado."));
        });

        pecaOuInsumoDaOrdemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PecaOuInsumoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
        pecaInsumoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIsNotEmDiagnostico()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var pecaInsumoRepositoryMock = new Mock<IPecaInsumoRepository>();
        var pecaOuInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico(emDiagnostico: false);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        var useCase = new AdicionarPecaInsumoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            pecaInsumoRepositoryMock.Object,
            pecaOuInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServicoId: ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Somente ordens de servico em diagnostico podem receber pecas e insumos."));
        });

        pecaInsumoRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        pecaOuInsumoDaOrdemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PecaOuInsumoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
        pecaInsumoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenQuantidadeIsInvalid()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var pecaInsumoRepositoryMock = new Mock<IPecaInsumoRepository>();
        var pecaOuInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico();
        var pecaInsumo = CreatePecaInsumo();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = pecaInsumoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PecaInsumo>.Success(pecaInsumo));

        var useCase = new AdicionarPecaInsumoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            pecaInsumoRepositoryMock.Object,
            pecaOuInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id, pecaInsumo.Id, quantidade: 0));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description.Contains("quantidade", StringComparison.OrdinalIgnoreCase), Is.True);
        });

        pecaOuInsumoDaOrdemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PecaOuInsumoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
        pecaInsumoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenQuantidadeExceedsEstoque()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var pecaInsumoRepositoryMock = new Mock<IPecaInsumoRepository>();
        var pecaOuInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico();
        var pecaInsumo = CreatePecaInsumo(quantidadeEstoque: 2);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = pecaInsumoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PecaInsumo>.Success(pecaInsumo));

        var useCase = new AdicionarPecaInsumoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            pecaInsumoRepositoryMock.Object,
            pecaOuInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id, pecaInsumo.Id, quantidade: 3));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A quantidade informada é maior que o estoque disponível."));
        });

        pecaOuInsumoDaOrdemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PecaOuInsumoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
        pecaInsumoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var pecaInsumoRepositoryMock = new Mock<IPecaInsumoRepository>();
        var pecaOuInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico();
        var pecaInsumo = CreatePecaInsumo();
        PecaOuInsumoDaOrdemDeServico? pecaOuInsumoAdicionado = null;
        PecaInsumo? pecaInsumoAtualizado = null;

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = pecaInsumoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PecaInsumo>.Success(pecaInsumo));
        _ = pecaOuInsumoDaOrdemRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<PecaOuInsumoDaOrdemDeServico>(), It.IsAny<CancellationToken>()))
            .Callback<PecaOuInsumoDaOrdemDeServico, CancellationToken>((item, _) => pecaOuInsumoAdicionado = item)
            .Returns(Task.CompletedTask);
        _ = pecaInsumoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()))
            .Callback<PecaInsumo, CancellationToken>((item, _) => pecaInsumoAtualizado = item)
            .Returns(Task.CompletedTask);

        var useCase = new AdicionarPecaInsumoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            pecaInsumoRepositoryMock.Object,
            pecaOuInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id, pecaInsumo.Id, quantidade: 3));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(pecaOuInsumoAdicionado, Is.Not.Null);
            Assert.That(pecaInsumoAtualizado, Is.Not.Null);
        });

        pecaOuInsumoDaOrdemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PecaOuInsumoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Once);
        pecaInsumoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(pecaOuInsumoAdicionado!.Id));
            Assert.That(response.OrdemServicoId, Is.EqualTo(ordemServico.Id));
            Assert.That(response.PecaInsumoId, Is.EqualTo(pecaInsumo.Id));
            Assert.That(response.Nome, Is.EqualTo(pecaInsumo.Nome));
            Assert.That(response.Codigo, Is.EqualTo(pecaInsumo.Codigo));
            Assert.That(response.Descricao, Is.EqualTo(pecaInsumo.Descricao));
            Assert.That(response.PrecoUnitario, Is.EqualTo(pecaInsumo.PrecoUnitario));
            Assert.That(response.Quantidade, Is.EqualTo(3));
            Assert.That(response.ValorTotal, Is.EqualTo(pecaInsumo.PrecoUnitario * 3));
            Assert.That(pecaInsumoAtualizado!.QuantidadeEstoque, Is.EqualTo(27));
        });
    }

    private static AdicionarPecaInsumoOrdemServicoCommand CreateCommand(
        Guid? ordemServicoId = null,
        Guid? pecaInsumoId = null,
        int quantidade = 1) =>
        new()
        {
            OrdemServicoId = ordemServicoId ?? Guid.NewGuid(),
            PecaInsumoId = pecaInsumoId ?? Guid.NewGuid(),
            Quantidade = quantidade
        };

    private static OrdemServico CreateOrdemServico(bool emDiagnostico = true)
    {
        var ordemResult = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Falha no ar-condicionado");

        Assert.Multiple(() =>
        {
            Assert.That(ordemResult.IsSuccess, Is.True);
            Assert.That(ordemResult.Value, Is.Not.Null);
        });

        var ordemServico = ordemResult.Value!;

        if (emDiagnostico)
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

    private static PecaInsumo CreatePecaInsumo(int quantidadeEstoque = 30)
    {
        var pecaInsumoResult = PecaInsumo.Create("Pastilha de freio", "PST-001", "Pastilha dianteira", 180m, quantidadeEstoque);

        Assert.Multiple(() =>
        {
            Assert.That(pecaInsumoResult.IsSuccess, Is.True);
            Assert.That(pecaInsumoResult.Value, Is.Not.Null);
        });

        return pecaInsumoResult.Value!;
    }
}
