using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.CriarUsuario;

public interface ICriarUsuarioUseCase
{
    Task<Result<CriarUsuarioResponse>> ExecuteAsync(CriarUsuarioCommand command, CancellationToken cancellationToken = default);
}
