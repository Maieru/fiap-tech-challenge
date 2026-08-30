using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AprovarExecucaoOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.AprovarExecucaoOrdemServico;

[TestFixture]
internal sealed class AprovarExecucaoOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.")));

        var useCase = new AprovarExecucaoOrdemServicoUseCase(ordemServicoRepositoryMock.Object, clienteRepositoryMock.Object);
        var result = await useCase.ExecuteAsync(CreateCommand());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
        });

        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIsNotAguardandoAprovacao()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var ordemServico = CreateOrdemServico(aguardandoAprovacao: false);
        var cliente = CreateCliente(ordemServico.ClienteId);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(ordemServico.ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));

        var useCase = new AprovarExecucaoOrdemServicoUseCase(ordemServicoRepositoryMock.Object, clienteRepositoryMock.Object);
        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id, CreateToken(cliente, ordemServico)));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Does.Contain("aguardando"));
        });

        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenTokenDoesNotMatch()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var ordemServico = CreateOrdemServico(aguardandoAprovacao: true);
        var cliente = CreateCliente(ordemServico.ClienteId);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(ordemServico.ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));

        var useCase = new AprovarExecucaoOrdemServicoUseCase(ordemServicoRepositoryMock.Object, clienteRepositoryMock.Object);
        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id, new string('0', 64)));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O token de acesso informado e invalido."));
            Assert.That(ordemServico.Status, Is.EqualTo(StatusOrdemServico.AguardandoAprovacao));
            Assert.That(ordemServico.DataInicioExecucao, Is.Null);
        });

        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenOrdemServicoIsAguardandoAprovacao()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var ordemServico = CreateOrdemServico(aguardandoAprovacao: true);
        var cliente = CreateCliente(ordemServico.ClienteId);
        OrdemServico? ordemServicoAtualizada = null;

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(ordemServico.ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()))
            .Callback<OrdemServico, CancellationToken>((item, _) => ordemServicoAtualizada = item)
            .Returns(Task.CompletedTask);

        var useCase = new AprovarExecucaoOrdemServicoUseCase(ordemServicoRepositoryMock.Object, clienteRepositoryMock.Object);
        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id, CreateToken(cliente, ordemServico)));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(ordemServicoAtualizada, Is.Not.Null);
        });

        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(ordemServico.Id));
            Assert.That(response.Status, Is.EqualTo(StatusOrdemServico.EmExecucao));
            Assert.That(response.DataInicioExecucao, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-5)));
        });
    }

    private static AprovarExecucaoOrdemServicoCommand CreateCommand(Guid? ordemServicoId = null, string? token = null) =>
        new()
        {
            OrdemServicoId = ordemServicoId ?? Guid.NewGuid(),
            Token = token ?? new string('0', 64)
        };

    private static string CreateToken(Cliente cliente, OrdemServico ordemServico) =>
        CpfAccessToken.Create(cliente.Cpf!, ordemServico.CodigoAprovacao);

    private static Cliente CreateCliente(Guid id)
    {
        var cpf = Cpf.Create("52998224725").Value!;
        var telefone = Telefone.Create("11987654321").Value!;

        return Cliente.Rehydrate(id, "Cliente Teste", cpf, null, telefone, null).Value!;
    }

    private static OrdemServico CreateOrdemServico(bool aguardandoAprovacao)
    {
        var ordemServicoResult = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Falha no sistema de injecao");

        Assert.Multiple(() =>
        {
            Assert.That(ordemServicoResult.IsSuccess, Is.True);
            Assert.That(ordemServicoResult.Value, Is.Not.Null);
        });

        var ordemServico = ordemServicoResult.Value!;

        if (aguardandoAprovacao)
        {
            var iniciarDiagnosticoResult = ordemServico.IniciarDiagnostico();
            var aguardarAprovacaoResult = ordemServico.AguardarAprovacao();

            Assert.Multiple(() =>
            {
                Assert.That(iniciarDiagnosticoResult.IsSuccess, Is.True);
                Assert.That(aguardarAprovacaoResult.IsSuccess, Is.True);
            });
        }

        return ordemServico;
    }
}

