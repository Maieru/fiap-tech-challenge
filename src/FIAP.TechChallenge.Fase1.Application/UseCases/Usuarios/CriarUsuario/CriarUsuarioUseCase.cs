using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.CriarUsuario;

public sealed class CriarUsuarioUseCase(IUsuarioRepository usuarioRepository, IPasswordHasher passwordHasher) : ICriarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async Task<Result<CriarUsuarioResponse>> ExecuteAsync(CriarUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        var senhaResult = ValidateSenha(command.Senha);

        if (!senhaResult.IsSuccess)
            return Result<CriarUsuarioResponse>.Failure(senhaResult.Error);

        var senhaCriptografada = _passwordHasher.HashPassword(command.Senha);

        var usuarioResult = Usuario.Create(command.Usuario, senhaCriptografada);
        if (!usuarioResult.IsSuccess || usuarioResult.Value is null)
            return Result<CriarUsuarioResponse>.Failure(usuarioResult.Error);

        var usuario = usuarioResult.Value;

        var usuarioJaExiste = await _usuarioRepository.ExistsByLoginAsync(usuario.Login, cancellationToken);
        if (usuarioJaExiste)
            return Result<CriarUsuarioResponse>.Failure(Error.Conflict("Já existe um usuário cadastrado com este nome de usuário."));

        await _usuarioRepository.AddAsync(usuario, cancellationToken);

        return Result<CriarUsuarioResponse>.Success(new CriarUsuarioResponse
        {
            Id = usuario.Id,
            Usuario = usuario.Login,
        });
    }

    private static Result<bool> ValidateSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            return Result<bool>.Failure(new Error("A senha é obrigatória."));

        if (senha.Trim().Length < 8)
            return Result<bool>.Failure(new Error("A senha deve ter no mínimo 8 caracteres."));

        return Result<bool>.Success(true);
    }

}
