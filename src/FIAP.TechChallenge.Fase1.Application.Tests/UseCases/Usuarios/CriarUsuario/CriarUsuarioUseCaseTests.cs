using FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.CriarUsuario;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Usuarios.CriarUsuario;

[TestFixture]
internal sealed class CriarUsuarioUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenSenhaIsInvalid()
    {
        var repositoryMock = new Mock<IUsuarioRepository>();
        var hasherMock = new Mock<IPasswordHasher>();
        var useCase = new CriarUsuarioUseCase(repositoryMock.Object, hasherMock.Object);
        var command = CreateCommand(senha: "123");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A senha deve ter no mínimo 8 caracteres."));
        });

        repositoryMock.Verify(x => x.ExistsByLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
        hasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenUsuarioAlreadyExists()
    {
        var repositoryMock = new Mock<IUsuarioRepository>();
        var hasherMock = new Mock<IPasswordHasher>();

        _ = repositoryMock
            .Setup(x => x.ExistsByLoginAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _ = hasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("HASH_FIXO");

        var useCase = new CriarUsuarioUseCase(repositoryMock.Object, hasherMock.Object);
        var command = CreateCommand(usuario: "Admin");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Já existe um usuário cadastrado com este nome de usuário."));
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.Conflict));
        });

        repositoryMock.Verify(x => x.ExistsByLoginAsync("admin", It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
        hasherMock.Verify(x => x.HashPassword("SenhaForte@123"), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var repositoryMock = new Mock<IUsuarioRepository>();
        var hasherMock = new Mock<IPasswordHasher>();
        Usuario? addedUsuario = null;
        const string hashEsperado = "HASH_FIXO";

        _ = repositoryMock
            .Setup(x => x.ExistsByLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _ = hasherMock
            .Setup(x => x.HashPassword("SenhaForte@123"))
            .Returns(hashEsperado);
        _ = repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Callback<Usuario, CancellationToken>((usuario, _) => addedUsuario = usuario)
            .Returns(Task.CompletedTask);

        var useCase = new CriarUsuarioUseCase(repositoryMock.Object, hasherMock.Object);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(addedUsuario, Is.Not.Null);
        });

        repositoryMock.Verify(x => x.ExistsByLoginAsync("admin", It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        hasherMock.Verify(x => x.HashPassword("SenhaForte@123"), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(addedUsuario!.Id));
            Assert.That(response.Usuario, Is.EqualTo("admin"));
            Assert.That(response.SenhaCriptografada, Is.EqualTo(hashEsperado));
            Assert.That(response.SenhaCriptografada, Is.EqualTo(addedUsuario.Senha));
        });
    }

    private static CriarUsuarioCommand CreateCommand(string usuario = "admin", string senha = "SenhaForte@123") =>
        new()
        {
            Usuario = usuario,
            Senha = senha
        };
}
