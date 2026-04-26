using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ExcluirCliente;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Clientes.ExcluirCliente;

[TestFixture]
internal sealed class ExcluirClienteUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldCallDelete_WhenClienteExists()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var cliente = CreateCliente();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));

        var useCase = new ExcluirClienteUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ExcluirClienteCommand { Id = cliente.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Id, Is.EqualTo(cliente.Id));
        });
        repositoryMock.Verify(x => x.DeleteAsync(cliente, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenClienteDoesNotExist()
    {
        var repositoryMock = new Mock<IClienteRepository>();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Failure(new Error("Cliente nao encontrado.", ErrorCode.NotFound)));

        var useCase = new ExcluirClienteUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ExcluirClienteCommand { Id = Guid.NewGuid() });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.NotFound));
        });
        repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Cliente CreateCliente()
    {
        var cpf = Cpf.Create("52998224725").Value!;
        var telefone = Telefone.Create("11987654321").Value!;

        return Cliente.Create("Cliente Teste", cpf, null, telefone, null).Value!;
    }
}
