namespace FIAP.TechChallenge.OS.Application.Abstractions;
public interface IAcessoClienteService
{
    Task<string> EmitirAsync(Guid ordemServicoId, CancellationToken cancellationToken);
    Task<bool> ValidarAsync(Guid ordemServicoId, string token, CancellationToken cancellationToken);
}
