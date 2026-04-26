using FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.ExcluirUsuario;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Usuarios.ExcluirUsuario;

[TestFixture]
internal sealed class ExcluirUsuarioUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldCallDelete_WhenUsuarioExists()
    {
        var repositoryMock = new Mock<IUsuarioRepository>();
        var usuario = Usuario.Create("usuario", "senha-criptografada").Value!;

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Usuario>.Success(usuario));

        var useCase = new ExcluirUsuarioUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ExcluirUsuarioCommand { Id = usuario.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Id, Is.EqualTo(usuario.Id));
        });
        repositoryMock.Verify(x => x.DeleteAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenUsuarioDoesNotExist()
    {
        var repositoryMock = new Mock<IUsuarioRepository>();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Usuario>.Failure(new Error("Usuario nao encontrado.", ErrorCode.NotFound)));

        var useCase = new ExcluirUsuarioUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ExcluirUsuarioCommand { Id = Guid.NewGuid() });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.NotFound));
        });
        repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
