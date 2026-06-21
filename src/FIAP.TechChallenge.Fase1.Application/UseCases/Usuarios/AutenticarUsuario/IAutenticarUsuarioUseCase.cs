using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.AutenticarUsuario;

public interface IAutenticarUsuarioUseCase
{
    Task<Result<AutenticarUsuarioResponse>> ExecuteAsync(
        AutenticarUsuarioCommand command,
        CancellationToken cancellationToken = default);
}

