using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.AutenticarUsuario;

public sealed class AutenticarUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IAutenticarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<Result<AutenticarUsuarioResponse>> ExecuteAsync(AutenticarUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        var senha = command.Senha?.Trim();

        if (string.IsNullOrWhiteSpace(command.Usuario))
            return Result<AutenticarUsuarioResponse>.Failure(new Error("O usuario e obrigatorio."));

        if (string.IsNullOrWhiteSpace(senha))
            return Result<AutenticarUsuarioResponse>.Failure(new Error("A senha e obrigatoria."));

        var loginResult = Usuario.ValidateLogin(command.Usuario);

        if (!loginResult.IsSuccess || loginResult.Value is null)
            return Result<AutenticarUsuarioResponse>.Failure(loginResult.Error);

        var usuario = await _usuarioRepository.GetByLoginAsync(loginResult.Value, cancellationToken);

        if (usuario is null)
            return Result<AutenticarUsuarioResponse>.Failure(Error.Unauthorized("Usuario ou senha invalidos."));

        var senhaValida = _passwordHasher.VerifyHashedPassword(usuario.Senha, senha);

        if (!senhaValida)
            return Result<AutenticarUsuarioResponse>.Failure(Error.Unauthorized("Usuario ou senha invalidos."));

        var token = _tokenService.GenerateToken(usuario);

        return Result<AutenticarUsuarioResponse>.Success(new AutenticarUsuarioResponse
        {
            Token = token,
            ExpiresInSeconds = _tokenService.AccessTokenLifetimeSeconds
        });
    }
}

