using FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.AutenticarUsuario;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Usuarios.AutenticarUsuario;

[TestFixture]
internal sealed class AutenticarUsuarioUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenUsuarioIsEmpty()
    {
        var repositoryMock = new Mock<IUsuarioRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenServiceMock = new Mock<ITokenService>();
        var useCase = new AutenticarUsuarioUseCase(repositoryMock.Object, passwordHasherMock.Object, tokenServiceMock.Object);

        var result = await useCase.ExecuteAsync(new AutenticarUsuarioCommand { Senha = "senha" });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O usuario e obrigatorio."));
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.BadRequest));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCredentialsAreInvalid()
    {
        var repositoryMock = new Mock<IUsuarioRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenServiceMock = new Mock<ITokenService>();

        _ = repositoryMock
            .Setup(x => x.GetByLoginAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var useCase = new AutenticarUsuarioUseCase(repositoryMock.Object, passwordHasherMock.Object, tokenServiceMock.Object);

        var result = await useCase.ExecuteAsync(new AutenticarUsuarioCommand
        {
            Usuario = "Admin",
            Senha = "SenhaForte@123"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Usuario ou senha invalidos."));
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.Unauthorized));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenUsuarioHasUnsafeCharacters()
    {
        var repositoryMock = new Mock<IUsuarioRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenServiceMock = new Mock<ITokenService>();
        var useCase = new AutenticarUsuarioUseCase(repositoryMock.Object, passwordHasherMock.Object, tokenServiceMock.Object);

        var result = await useCase.ExecuteAsync(new AutenticarUsuarioCommand
        {
            Usuario = "John Doe AND 1=1 --",
            Senha = "SenhaForte@123"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O usuario deve conter apenas letras, numeros, ponto, hifen ou underscore."));
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.BadRequest));
        });

        repositoryMock.Verify(x => x.GetByLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        passwordHasherMock.Verify(x => x.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCredentialsAreValid()
    {
        var repositoryMock = new Mock<IUsuarioRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var tokenServiceMock = new Mock<ITokenService>();

        var usuarioResult = Usuario.Create("admin", "HASH");
        Assert.That(usuarioResult.IsSuccess, Is.True);
        var usuario = usuarioResult.Value!;

        _ = repositoryMock
            .Setup(x => x.GetByLoginAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _ = passwordHasherMock
            .Setup(x => x.VerifyHashedPassword("HASH", "SenhaForte@123"))
            .Returns(true);
        _ = tokenServiceMock
            .SetupGet(x => x.AccessTokenLifetimeSeconds)
            .Returns(3600);
        _ = tokenServiceMock
            .Setup(x => x.GenerateToken(usuario))
            .Returns("jwt_token");

        var useCase = new AutenticarUsuarioUseCase(repositoryMock.Object, passwordHasherMock.Object, tokenServiceMock.Object);

        var result = await useCase.ExecuteAsync(new AutenticarUsuarioCommand
        {
            Usuario = "Admin",
            Senha = "SenhaForte@123"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Token, Is.EqualTo("jwt_token"));
            Assert.That(result.Value.TipoToken, Is.EqualTo("Bearer"));
            Assert.That(result.Value.ExpiresInSeconds, Is.EqualTo(3600));
        });
    }
}
