using FIAP.TechChallenge.OS.Domain.Abstractions;
namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.EmitirAcessoCliente;
public interface IEmitirAcessoClienteUseCase { Task<Result<EmitirAcessoClienteResponse>> ExecuteAsync(EmitirAcessoClienteCommand command, CancellationToken cancellationToken = default); }
