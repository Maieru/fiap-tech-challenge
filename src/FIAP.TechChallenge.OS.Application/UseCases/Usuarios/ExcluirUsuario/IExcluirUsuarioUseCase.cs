using FIAP.TechChallenge.OS.Domain.Abstractions;

namespace FIAP.TechChallenge.OS.Application.UseCases.Usuarios.ExcluirUsuario;

public interface IExcluirUsuarioUseCase
{
    Task<Result<ExcluirUsuarioResponse>> ExecuteAsync(ExcluirUsuarioCommand command, CancellationToken cancellationToken = default);
}

