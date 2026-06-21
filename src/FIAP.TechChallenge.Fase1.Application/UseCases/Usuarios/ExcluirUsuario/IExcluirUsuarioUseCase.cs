using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.ExcluirUsuario;

public interface IExcluirUsuarioUseCase
{
    Task<Result<ExcluirUsuarioResponse>> ExecuteAsync(ExcluirUsuarioCommand command, CancellationToken cancellationToken = default);
}

