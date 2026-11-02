using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Usuarios.CriarUsuario;

public interface ICriarUsuarioUseCase
{
    Task<Result<CriarUsuarioResponse>> ExecuteAsync(CriarUsuarioCommand command, CancellationToken cancellationToken = default);
}

