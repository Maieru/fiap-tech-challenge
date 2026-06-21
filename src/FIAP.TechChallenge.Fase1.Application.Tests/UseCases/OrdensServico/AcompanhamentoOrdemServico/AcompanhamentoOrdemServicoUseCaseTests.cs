using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AcompanhamentoOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.AcompanhamentoOrdemServico;

[TestFixture]
internal sealed class AcompanhamentoOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIdIsEmpty()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var pecaInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();

        var useCase = new AcompanhamentoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            clienteRepositoryMock.Object,
            veiculoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
            pecaInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(new AcompanhamentoOrdemServicoCommand { OrdemServicoId = Guid.Empty });

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
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var pecaInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();
        var ordemServicoId = Guid.NewGuid();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(ordemServicoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.", ErrorCode.NotFound)));

        var useCase = new AcompanhamentoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            clienteRepositoryMock.Object,
            veiculoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
            pecaInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(new AcompanhamentoOrdemServicoCommand { OrdemServicoId = ordemServicoId });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.NotFound));
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        veiculoRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        servicoDaOrdemRepositoryMock.Verify(x => x.GetByOrdemServicoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        pecaInsumoDaOrdemRepositoryMock.Verify(x => x.GetByOrdemServicoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenClienteDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var pecaInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico(Guid.NewGuid(), Guid.NewGuid(), StatusOrdemServico.Recebida);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(ordemServico.ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Failure(new Error("Cliente nao encontrado.", ErrorCode.NotFound)));

        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(ordemServico.VeiculoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(CreateVeiculo(ordemServico.VeiculoId, ordemServico.ClienteId)));

        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Success([]));

        _ = pecaInsumoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>>.Success([]));

        var useCase = new AcompanhamentoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            clienteRepositoryMock.Object,
            veiculoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
            pecaInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(new AcompanhamentoOrdemServicoCommand { OrdemServicoId = ordemServico.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Cliente nao encontrado."));
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.NotFound));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenOrdemServicoExists()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var pecaInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ordemServico = CreateOrdemServico(clienteId, veiculoId, StatusOrdemServico.EmDiagnostico);
        var cliente = CreateCliente(clienteId, "Cliente Acompanhamento");
        var veiculo = CreateVeiculo(veiculoId, clienteId);
        var servicoDaOrdem = CreateServicoDaOrdem(ordemServico.Id, "Alinhamento", 180m, 2);
        var pecaInsumoDaOrdem = CreatePecaInsumoDaOrdem(ordemServico.Id, "Filtro de ar", "flt-001", 49m, 3);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));

        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(veiculoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(veiculo));

        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Success(new[] { servicoDaOrdem }));

        _ = pecaInsumoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>>.Success(new[] { pecaInsumoDaOrdem }));

        var useCase = new AcompanhamentoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            clienteRepositoryMock.Object,
            veiculoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object,
            pecaInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(new AcompanhamentoOrdemServicoCommand { OrdemServicoId = ordemServico.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(ordemServico.Id));
            Assert.That(result.Value.ClienteId, Is.EqualTo(clienteId));
            Assert.That(result.Value.ClienteNome, Is.EqualTo("Cliente Acompanhamento"));
            Assert.That(result.Value.VeiculoId, Is.EqualTo(veiculoId));
            Assert.That(result.Value.VeiculoMarca, Is.EqualTo("Toyota"));
            Assert.That(result.Value.VeiculoModelo, Is.EqualTo("Corolla"));
            Assert.That(result.Value.VeiculoPlaca, Is.EqualTo("ABC1234"));
            Assert.That(result.Value.VeiculoAno, Is.EqualTo(2024));
            Assert.That(result.Value.Status, Is.EqualTo(StatusOrdemServico.EmDiagnostico));
            Assert.That(result.Value.Servicos, Has.Count.EqualTo(1));
            Assert.That(result.Value.PecasInsumos, Has.Count.EqualTo(1));
            Assert.That(result.Value.Servicos.First().Descricao, Is.EqualTo("Alinhamento"));
            Assert.That(result.Value.PecasInsumos.First().Codigo, Is.EqualTo("FLT-001"));
            Assert.That(result.Value.ValorTotalServicos, Is.EqualTo(360m));
            Assert.That(result.Value.ValorTotalPecasInsumos, Is.EqualTo(147m));
            Assert.That(result.Value.ValorTotalOrdemServico, Is.EqualTo(507m));
        });
    }

    private static OrdemServico CreateOrdemServico(Guid clienteId, Guid veiculoId, StatusOrdemServico status)
    {
        var ordemServicoResult = OrdemServico.Create(clienteId, veiculoId, "Ruido no sistema de direcao");

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

    private static Cliente CreateCliente(Guid clienteId, string nome)
    {
        var cpfResult = Cpf.Create("52998224725");
        var telefoneResult = Telefone.Create("11987654321");
        var emailResult = Email.Create("cliente@email.com");

        Assert.Multiple(() =>
        {
            Assert.That(cpfResult.IsSuccess, Is.True);
            Assert.That(telefoneResult.IsSuccess, Is.True);
            Assert.That(emailResult.IsSuccess, Is.True);
        });

        var clienteResult = Cliente.Rehydrate(clienteId, nome, cpfResult.Value, null, telefoneResult.Value!, emailResult.Value);

        Assert.Multiple(() =>
        {
            Assert.That(clienteResult.IsSuccess, Is.True);
            Assert.That(clienteResult.Value, Is.Not.Null);
        });

        return clienteResult.Value!;
    }

    private static Veiculo CreateVeiculo(Guid veiculoId, Guid clienteId)
    {
        var placaResult = Placa.Create("ABC1234");

        Assert.Multiple(() =>
        {
            Assert.That(placaResult.IsSuccess, Is.True);
            Assert.That(placaResult.Value, Is.Not.Null);
        });

        var veiculoResult = Veiculo.Rehydrate(veiculoId, clienteId, placaResult.Value!, "Toyota", "Corolla", 2024);

        Assert.Multiple(() =>
        {
            Assert.That(veiculoResult.IsSuccess, Is.True);
            Assert.That(veiculoResult.Value, Is.Not.Null);
        });

        return veiculoResult.Value!;
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

