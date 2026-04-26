using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.ExcluirUsuario;

public sealed class ExcluirUsuarioUseCase(IUsuarioRepository usuarioRepository) : IExcluirUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

    public async Task<Result<ExcluirUsuarioResponse>> ExecuteAsync(ExcluirUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        var usuarioResult = await _usuarioRepository.GetByIdAsync(command.Id, cancellationToken);

        if (!usuarioResult.IsSuccess || usuarioResult.Value is null)
            return Result<ExcluirUsuarioResponse>.Failure(usuarioResult.Error);

        await _usuarioRepository.DeleteAsync(usuarioResult.Value, cancellationToken);

        return Result<ExcluirUsuarioResponse>.Success(new ExcluirUsuarioResponse { Id = command.Id });
    }
}
