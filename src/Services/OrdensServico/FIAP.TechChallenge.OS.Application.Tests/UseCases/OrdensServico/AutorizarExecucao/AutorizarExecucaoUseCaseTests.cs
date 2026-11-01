using FIAP.TechChallenge.OS.Application.Abstractions;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.AutorizarExecucao;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Interfaces;
using Maieru.FiapTechChallenge.Chassis;
using Moq;

namespace FIAP.TechChallenge.OS.Application.Tests.UseCases.OrdensServico.AutorizarExecucao;

public sealed class AutorizarExecucaoUseCaseTests
{
    [TestCase("none")]
    [TestCase("save")]
    [TestCase("publish")]
    [TestCase("commit")]
    public async Task ExecuteAsync_ShouldRespectTransactionOrderAndPropagateFailures(string failure)
    {
        var order = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Motor com falha").Value!;
        _ = order.RegistrarDiagnosticoIniciado(DateTime.UtcNow);
        _ = order.RegistrarDiagnosticoConcluido(DateTime.UtcNow);
        var calls = new List<string>();
        var repository = new Mock<IOrdemServicoRepository>();
        _ = repository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Result<OrdemServico>.Success(order));
        Task Step(string step)
        { calls.Add(step); return failure == step ? Task.FromException(new IOException(step)) : Task.CompletedTask; }
        _ = repository.Setup(x => x.UpdateAsync(order, It.IsAny<HistoricoOrdem>(), It.IsAny<CancellationToken>())).Returns(() => Step("save"));
        var transaction = new Mock<ITransaction>();
        _ = transaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(() => Step("commit"));
        _ = transaction.Setup(x => x.DisposeAsync()).Callback(() => calls.Add("dispose")).Returns(ValueTask.CompletedTask);
        var work = new Mock<IUnitOfWork>();
        _ = work.Setup(x => x.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        var messaging = new Mock<IMessagingService>();
        _ = messaging.Setup(x => x.PublishAsync(OSMessaging.Topic, order.Id.ToString("D"),
            It.IsAny<MessageEnvelope<ExecucaoAutorizada>>(), It.IsAny<CancellationToken>())).Returns(() => Step("publish"));
        var useCase = new AutorizarExecucaoUseCase(repository.Object, work.Object, messaging.Object, new MessageContext(), TimeProvider.System);
        if (failure == "none")
        {
            var result = await useCase.ExecuteAsync(new(order.Id));
            Assert.That(result.IsSuccess, Is.True);
        }
        else
            _ = Assert.ThrowsAsync<IOException>(() => useCase.ExecuteAsync(new(order.Id)));
        var expected = failure switch
        {
            "save" => new[] { "save", "dispose" },
            "publish" => new[] { "save", "publish", "dispose" },
            _ => new[] { "save", "publish", "commit", "dispose" }
        };
        Assert.That(calls, Is.EqualTo(expected));
    }
}
