using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.SolicitarFluxo;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.OS.Application.Tests;

public sealed class FluxoOrdemUseCaseTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task Execute_ShouldRejectMissingOperator(string? actor)
    {
        var repository = new Mock<IFluxoOrdemRepository>(MockBehavior.Strict);
        var result = await new SolicitarFluxoUseCase(repository.Object).ExecuteAsync(Guid.NewGuid(), "Autorizacao", actor, default);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.Forbidden));
    }

    [Test]
    public async Task Execute_ShouldRejectMissingId()
    {
        var repository = new Mock<IFluxoOrdemRepository>(MockBehavior.Strict);
        var result = await new SolicitarFluxoUseCase(repository.Object).ExecuteAsync(Guid.Empty, "Cancelamento", "operator", default);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.BadRequest));
    }

    [Test]
    public async Task Execute_ShouldPassIdentityToRepository()
    {
        var repository = new Mock<IFluxoOrdemRepository>();
        var id = Guid.NewGuid();
        var expected = new SolicitacaoOrdem(Guid.NewGuid(), "Autorizacao");
        _ = repository.Setup(x => x.SolicitarAsync(id, "Autorizacao", "operator", It.IsAny<CancellationToken>())).ReturnsAsync(Result<SolicitacaoOrdem>.Success(expected));
        var result = await new SolicitarFluxoUseCase(repository.Object).ExecuteAsync(id, "Autorizacao", "operator", default);
        Assert.That(result.Value, Is.EqualTo(expected));
        repository.Verify(x => x.SolicitarAsync(id, "Autorizacao", "operator", It.IsAny<CancellationToken>()), Times.Once);
    }
}
