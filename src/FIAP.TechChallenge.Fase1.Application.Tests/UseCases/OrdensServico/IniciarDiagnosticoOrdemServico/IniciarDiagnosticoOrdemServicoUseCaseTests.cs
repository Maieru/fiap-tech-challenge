using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;

[TestFixture]
internal sealed class IniciarDiagnosticoOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.")));

        var useCase = new IniciarDiagnosticoOrdemServicoUseCase(ordemServicoRepositoryMock.Object);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
        });

        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIsNotRecebida()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var ordemServico = CreateOrdemServico();
        var iniciarDiagnosticoResult = ordemServico.IniciarDiagnostico();

        Assert.Multiple(() =>
        {
            Assert.That(iniciarDiagnosticoResult.IsSuccess, Is.True);
            Assert.That(iniciarDiagnosticoResult.Value, Is.True);
        });

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        var useCase = new IniciarDiagnosticoOrdemServicoUseCase(ordemServicoRepositoryMock.Object);
        var command = CreateCommand(ordemServico.Id);

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Somente ordens de serviço recebidas podem iniciar diagnóstico."));
        });

        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenOrdemServicoIsRecebida()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var ordemServico = CreateOrdemServico();
        OrdemServico? updatedOrdemServico = null;

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()))
            .Callback<OrdemServico, CancellationToken>((item, _) => updatedOrdemServico = item)
            .Returns(Task.CompletedTask);

        var useCase = new IniciarDiagnosticoOrdemServicoUseCase(ordemServicoRepositoryMock.Object);
        var command = CreateCommand(ordemServico.Id);

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(updatedOrdemServico, Is.Not.Null);
        });

        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(ordemServico.Id));
            Assert.That(response.Status, Is.EqualTo(StatusOrdemServico.EmDiagnostico));
            Assert.That(response.DataInicioDiagnostico, Is.Not.EqualTo(default(DateTime)));
            Assert.That(response.DataInicioDiagnostico, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-5)));
        });
    }

    private static IniciarDiagnosticoOrdemServicoCommand CreateCommand(Guid? ordemServicoId = null) =>
        new()
        {
            OrdemServicoId = ordemServicoId ?? Guid.NewGuid()
        };

    private static OrdemServico CreateOrdemServico()
    {
        var ordemServicoResult = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Problema no sistema de ignicao");

        Assert.Multiple(() =>
        {
            Assert.That(ordemServicoResult.IsSuccess, Is.True);
            Assert.That(ordemServicoResult.Value, Is.Not.Null);
        });

        return ordemServicoResult.Value!;
    }
}
