using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ListarOrdensServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.ListarOrdensServico;

[TestFixture]
internal sealed class ListarOrdensServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenPaginationIsInvalid()
    {
        var repositoryMock = new Mock<IOrdemServicoRepository>();
        var useCase = new ListarOrdensServicoUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarOrdensServicoCommand { PageNumber = 0, PageSize = 10 });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O numero da pagina deve ser maior que zero."));
        });

        repositoryMock.Verify(
            x => x.GetPagedAsync(
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyCollection<StatusOrdemServico>>(),
                It.IsAny<SortDirection?>(),
                It.IsAny<SortDirection?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenClienteIdIsEmpty()
    {
        var repositoryMock = new Mock<IOrdemServicoRepository>();
        var useCase = new ListarOrdensServicoUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarOrdensServicoCommand { ClienteId = Guid.Empty, PageNumber = 1, PageSize = 10 });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O identificador do cliente deve ser valido."));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenVeiculoIdIsEmpty()
    {
        var repositoryMock = new Mock<IOrdemServicoRepository>();
        var useCase = new ListarOrdensServicoUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarOrdensServicoCommand { VeiculoId = Guid.Empty, PageNumber = 1, PageSize = 10 });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O identificador do veiculo deve ser valido."));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenListingWithFilters()
    {
        var repositoryMock = new Mock<IOrdemServicoRepository>();
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ordem = CreateOrdemServico(clienteId, veiculoId, StatusOrdemServico.EmDiagnostico, "Ruido na direcao");

        _ = repositoryMock
            .Setup(x => x.GetPagedAsync(
                clienteId,
                veiculoId,
                It.Is<IReadOnlyCollection<StatusOrdemServico>>(status => status.SequenceEqual(new[] { StatusOrdemServico.EmDiagnostico, StatusOrdemServico.EmExecucao })),
                SortDirection.Asc,
                SortDirection.Desc,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<(IReadOnlyCollection<OrdemServico> OrdensServico, int TotalItems)>.Success((new[] { ordem }, 1)));

        var useCase = new ListarOrdensServicoUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarOrdensServicoCommand
        {
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            Status = [StatusOrdemServico.EmDiagnostico, StatusOrdemServico.EmExecucao],
            StatusSortDirection = SortDirection.Asc,
            DataAberturaSortDirection = SortDirection.Desc,
            PageNumber = 1,
            PageSize = 10
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.TotalItems, Is.EqualTo(1));
            Assert.That(result.Value.OrdensServico, Has.Count.EqualTo(1));
            Assert.That(result.Value.OrdensServico.First().Id, Is.EqualTo(ordem.Id));
            Assert.That(result.Value.OrdensServico.First().ClienteId, Is.EqualTo(clienteId));
            Assert.That(result.Value.OrdensServico.First().VeiculoId, Is.EqualTo(veiculoId));
            Assert.That(result.Value.OrdensServico.First().Status, Is.EqualTo(StatusOrdemServico.EmDiagnostico));
            Assert.That(result.Value.OrdensServico.First().DataInicioDiagnostico, Is.Not.Null);
        });

        repositoryMock.Verify(
            x => x.GetPagedAsync(
                clienteId,
                veiculoId,
                It.Is<IReadOnlyCollection<StatusOrdemServico>>(status => status.SequenceEqual(new[] { StatusOrdemServico.EmDiagnostico, StatusOrdemServico.EmExecucao })),
                SortDirection.Asc,
                SortDirection.Desc,
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static OrdemServico CreateOrdemServico(Guid clienteId, Guid veiculoId, StatusOrdemServico status, string descricaoProblema)
    {
        var ordemResult = OrdemServico.Create(clienteId, veiculoId, descricaoProblema);

        Assert.Multiple(() =>
        {
            Assert.That(ordemResult.IsSuccess, Is.True);
            Assert.That(ordemResult.Value, Is.Not.Null);
        });

        var ordemServico = ordemResult.Value!;

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
}
