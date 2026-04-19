using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.FinalizarOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.FinalizarOrdemServico;

[TestFixture]
internal sealed class FinalizarOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.")));

        var useCase = new FinalizarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
            clienteRepositoryMock.Object,
            mailServiceMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
        });

        servicoDaOrdemRepositoryMock.Verify(x => x.GetByOrdemServicoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        mailServiceMock.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIsNotEmExecucao()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var ordemServico = CreateOrdemServico(emExecucao: false);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Success([]));

        var useCase = new FinalizarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
            clienteRepositoryMock.Object,
            mailServiceMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Does.Contain("execução"));
        });

        servicoDaOrdemRepositoryMock.Verify(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()), Times.Once);
        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        mailServiceMock.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenExistsServicoNaoConcluido()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var ordemServico = CreateOrdemServico(emExecucao: true);
        var servicoDaOrdem = CreateServicoDaOrdem(ordemServico.Id, concluido: false);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Success([servicoDaOrdem]));

        var useCase = new FinalizarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
            clienteRepositoryMock.Object,
            mailServiceMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Does.Contain("todos os serviços concluídos"));
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        mailServiceMock.Verify(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenClienteDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var ordemServico = CreateOrdemServico(emExecucao: true);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Success([]));
        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Failure(new Error("Cliente nao encontrado.")));

        var useCase = new FinalizarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
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
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var ordemServico = CreateOrdemServico(emExecucao: true);
        var cliente = CreateCliente(withEmail: true);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Success([]));
        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = mailServiceMock
            .Setup(x => x.SendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Failure(new Error("Falha no envio do email.")));

        var useCase = new FinalizarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
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
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var ordemServico = CreateOrdemServico(emExecucao: true);
        var cliente = CreateCliente(withEmail: false);
        OrdemServico? ordemServicoAtualizada = null;

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Success([]));
        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()))
            .Callback<OrdemServico, CancellationToken>((item, _) => ordemServicoAtualizada = item)
            .Returns(Task.CompletedTask);

        var useCase = new FinalizarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
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
            Assert.That(response.Status, Is.EqualTo(StatusOrdemServico.Finalizada));
            Assert.That(response.DataFinalizacao, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-5)));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_AndSendMail_WhenClienteHasEmail()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var mailServiceMock = new Mock<IMailService>();
        var ordemServico = CreateOrdemServico(emExecucao: true);
        var cliente = CreateCliente(withEmail: true);
        OrdemServico? ordemServicoAtualizada = null;

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Success([]));
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

        var useCase = new FinalizarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
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
            x => x.SendMail(cliente.Email!.Value, It.IsAny<string>(), It.Is<string>(body => body.Contains(ordemServico.Id.ToString(), StringComparison.OrdinalIgnoreCase))),
            Times.Once);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(ordemServico.Id));
            Assert.That(response.Status, Is.EqualTo(StatusOrdemServico.Finalizada));
            Assert.That(response.DataFinalizacao, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-5)));
        });
    }

    private static FinalizarOrdemServicoCommand CreateCommand(Guid? ordemServicoId = null) =>
        new()
        {
            OrdemServicoId = ordemServicoId ?? Guid.NewGuid()
        };

    private static OrdemServico CreateOrdemServico(bool emExecucao)
    {
        var ordemServicoResult = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Falha no sistema de injecao");

        Assert.Multiple(() =>
        {
            Assert.That(ordemServicoResult.IsSuccess, Is.True);
            Assert.That(ordemServicoResult.Value, Is.Not.Null);
        });

        var ordemServico = ordemServicoResult.Value!;

        if (emExecucao)
        {
            var iniciarDiagnosticoResult = ordemServico.IniciarDiagnostico();
            var aguardarAprovacaoResult = ordemServico.AguardarAprovacao();
            var aprovarOrcamentoResult = ordemServico.AprovarOrcamento();

            Assert.Multiple(() =>
            {
                Assert.That(iniciarDiagnosticoResult.IsSuccess, Is.True);
                Assert.That(aguardarAprovacaoResult.IsSuccess, Is.True);
                Assert.That(aprovarOrcamentoResult.IsSuccess, Is.True);
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

    private static ServicoDaOrdemDeServico CreateServicoDaOrdem(Guid ordemServicoId, bool concluido)
    {
        var servicoResult = Servico.Create("Troca de oleo", 100m);

        Assert.Multiple(() =>
        {
            Assert.That(servicoResult.IsSuccess, Is.True);
            Assert.That(servicoResult.Value, Is.Not.Null);
        });

        var servicoDaOrdemResult = concluido
            ? ServicoDaOrdemDeServico.Rehydrate(
                new ServicoDaOrdemDeServicoSnapshot(
                    Guid.NewGuid(),
                    ordemServicoId,
                    servicoResult.Value!.Id,
                    servicoResult.Value.Descricao,
                    servicoResult.Value.ValorUnitario,
                    1,
                    30,
                    true))
            : ServicoDaOrdemDeServico.Create(ordemServicoId, servicoResult.Value!, 1);

        Assert.Multiple(() =>
        {
            Assert.That(servicoDaOrdemResult.IsSuccess, Is.True);
            Assert.That(servicoDaOrdemResult.Value, Is.Not.Null);
        });

        return servicoDaOrdemResult.Value!;
    }
}
