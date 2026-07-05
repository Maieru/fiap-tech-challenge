using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.SolicitarAprovacaoOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.SolicitarAprovacaoOrdemServico;

[TestFixture]
internal sealed class SolicitarAprovacaoOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.")));

        var useCase = new SolicitarAprovacaoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            clienteRepositoryMock.Object,
            mailServiceMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        mailServiceMock.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIsNotEmDiagnostico()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var ordemServico = CreateOrdemServico(emDiagnostico: false);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        var useCase = new SolicitarAprovacaoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            clienteRepositoryMock.Object,
            mailServiceMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Somente ordens de serviço em diagnóstico podem aguardar aprovação."));
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        mailServiceMock.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenClienteDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var ordemServico = CreateOrdemServico();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Failure(new Error("Cliente nao encontrado.")));

        var useCase = new SolicitarAprovacaoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            clienteRepositoryMock.Object,
            mailServiceMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Cliente nao encontrado."));
        });

        mailServiceMock.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenSendMailFails()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var ordemServico = CreateOrdemServico();
        var cliente = CreateCliente(withEmail: true);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = mailServiceMock
            .Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Failure(new Error("Falha no envio do email.")));

        var useCase = new SolicitarAprovacaoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            clienteRepositoryMock.Object,
            mailServiceMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Falha no envio do email."));
        });

        mailServiceMock.Verify(x => x.SendMail(cliente.Email!.Value, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenClienteDoesNotHaveEmail()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var ordemServico = CreateOrdemServico();
        var cliente = CreateCliente(withEmail: false);
        OrdemServico? ordemServicoAtualizada = null;

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()))
            .Callback<OrdemServico, CancellationToken>((item, _) => ordemServicoAtualizada = item)
            .Returns(Task.CompletedTask);

        var useCase = new SolicitarAprovacaoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            clienteRepositoryMock.Object,
            mailServiceMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(ordemServicoAtualizada, Is.Not.Null);
        });

        mailServiceMock.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(ordemServico.Id));
            Assert.That(response.Status, Is.EqualTo(StatusOrdemServico.AguardandoAprovacao));
            Assert.That(response.DataEnvioAprovacao, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-5)));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_AndSendMail_WhenClienteHasEmail()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var ordemServico = CreateOrdemServico();
        var cliente = CreateCliente(withEmail: true);
        OrdemServico? ordemServicoAtualizada = null;

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = mailServiceMock
            .Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Success(true));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()))
            .Callback<OrdemServico, CancellationToken>((item, _) => ordemServicoAtualizada = item)
            .Returns(Task.CompletedTask);

        var useCase = new SolicitarAprovacaoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            clienteRepositoryMock.Object,
            mailServiceMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(ordemServicoAtualizada, Is.Not.Null);
        });

        mailServiceMock.Verify(
            x => x.SendMail(
                cliente.Email!.Value,
                It.IsAny<string>(),
                It.Is<string>(body =>
                    body.Contains(ordemServico.Id.ToString(), StringComparison.OrdinalIgnoreCase) &&
                    body.Contains(ordemServico.CodigoAprovacao.ToString(), StringComparison.OrdinalIgnoreCase))),
            Times.Once);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(ordemServico.Id));
            Assert.That(response.Status, Is.EqualTo(StatusOrdemServico.AguardandoAprovacao));
            Assert.That(response.DataEnvioAprovacao, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-5)));
        });
    }

    private static SolicitarAprovacaoOrdemServicoCommand CreateCommand(Guid? ordemServicoId = null) =>
        new()
        {
            OrdemServicoId = ordemServicoId ?? Guid.NewGuid()
        };

    private static OrdemServico CreateOrdemServico(bool emDiagnostico = true)
    {
        var ordemResult = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Falha no sistema eletrico");

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

    private static Cliente CreateCliente(bool withEmail)
    {
        var cpfResult = Cpf.Create("52998224725");
        var telefoneResult = Telefone.Create("11987654321");
        var emailResult = Email.Create("cliente@exemplo.com");

        Assert.Multiple(() =>
        {
            Assert.That(cpfResult.IsSuccess, Is.True);
            Assert.That(telefoneResult.IsSuccess, Is.True);
            Assert.That(emailResult.IsSuccess, Is.True);
        });

        var clienteResult = Cliente.Create(
            "Cliente Teste",
            cpfResult.Value!,
            null,
            telefoneResult.Value!,
            withEmail ? emailResult.Value : null);

        Assert.Multiple(() =>
        {
            Assert.That(clienteResult.IsSuccess, Is.True);
            Assert.That(clienteResult.Value, Is.Not.Null);
        });

        return clienteResult.Value!;
    }
}

