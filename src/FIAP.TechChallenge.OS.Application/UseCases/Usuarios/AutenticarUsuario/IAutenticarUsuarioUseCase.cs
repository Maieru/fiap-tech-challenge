using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Usuarios.AutenticarUsuario;

public interface IAutenticarUsuarioUseCase
{
    Task<Result<AutenticarUsuarioResponse>> ExecuteAsync(
        AutenticarUsuarioCommand command,
        CancellationToken cancellationToken = default);
}

