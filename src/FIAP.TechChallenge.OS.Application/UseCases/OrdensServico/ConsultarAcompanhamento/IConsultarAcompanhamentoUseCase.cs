using FIAP.TechChallenge.OS.Domain.Abstractions;
namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarAcompanhamento;
public interface IConsultarAcompanhamentoUseCase { Task<Result<AcompanhamentoResponse>> ExecuteAsync(ConsultarAcompanhamentoCommand command, CancellationToken cancellationToken = default); }
