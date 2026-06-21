using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.AdicionarServicoOrdemServico;

[TestFixture]
internal sealed class AdicionarServicoOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoRepositoryMock = new Mock<IServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.")));

        var useCase = new AdicionarServicoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
        });

        servicoRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        servicoDaOrdemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoRepositoryMock = new Mock<IServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = servicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Servico>.Failure(new Error("Servico nao encontrado.")));

        var useCase = new AdicionarServicoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServicoId: ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Servico nao encontrado."));
        });

        servicoDaOrdemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIsNotEmDiagnostico()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoRepositoryMock = new Mock<IServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico(emDiagnostico: false);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        var useCase = new AdicionarServicoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServicoId: ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Somente ordens de servico em diagnostico podem receber servicos."));
        });

        servicoRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        servicoDaOrdemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenQuantidadeIsInvalid()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoRepositoryMock = new Mock<IServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico();
        var servico = CreateServico();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = servicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Servico>.Success(servico));

        var useCase = new AdicionarServicoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id, servico.Id, quantidade: 0));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description.Contains("quantidade", StringComparison.OrdinalIgnoreCase), Is.True);
        });

        servicoDaOrdemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoRepositoryMock = new Mock<IServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico();
        var servico = CreateServico();
        ServicoDaOrdemDeServico? servicoAdicionado = null;

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = servicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Servico>.Success(servico));
        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()))
            .Callback<ServicoDaOrdemDeServico, CancellationToken>((item, _) => servicoAdicionado = item)
            .Returns(Task.CompletedTask);

        var useCase = new AdicionarServicoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id, servico.Id, quantidade: 3));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(servicoAdicionado, Is.Not.Null);
        });

        servicoDaOrdemRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(servicoAdicionado!.Id));
            Assert.That(response.OrdemServicoId, Is.EqualTo(ordemServico.Id));
            Assert.That(response.ServicoId, Is.EqualTo(servico.Id));
            Assert.That(response.Descricao, Is.EqualTo(servico.Descricao));
            Assert.That(response.ValorUnitario, Is.EqualTo(servico.ValorUnitario));
            Assert.That(response.Quantidade, Is.EqualTo(3));
            Assert.That(response.ValorTotal, Is.EqualTo(servico.ValorUnitario * 3));
        });
    }

    private static AdicionarServicoOrdemServicoCommand CreateCommand(
        Guid? ordemServicoId = null,
        Guid? servicoId = null,
        int quantidade = 1) =>
        new()
        {
            OrdemServicoId = ordemServicoId ?? Guid.NewGuid(),
            ServicoId = servicoId ?? Guid.NewGuid(),
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

    private static Servico CreateServico()
    {
        var servicoResult = Servico.Create("Troca de oleo", 120m);

        Assert.Multiple(() =>
        {
            Assert.That(servicoResult.IsSuccess, Is.True);
            Assert.That(servicoResult.Value, Is.Not.Null);
        });

        return servicoResult.Value!;
    }
}

