using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ListarOrdensServico;

public sealed class ListarOrdensServicoUseCase(IOrdemServicoRepository ordemServicoRepository) : IListarOrdensServicoUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository = ordemServicoRepository;

    public async Task<Result<ListarOrdensServicoResponse>> ExecuteAsync(ListarOrdensServicoCommand command, CancellationToken cancellationToken = default)
    {
        var paginationValidationResult = ValidatePagination(command.PageNumber, command.PageSize);

        if (!paginationValidationResult.IsSuccess)
            return Result<ListarOrdensServicoResponse>.Failure(paginationValidationResult.Error);

        if (command.ClienteId.HasValue && command.ClienteId.Value == Guid.Empty)
            return Result<ListarOrdensServicoResponse>.Failure(new Error("O identificador do cliente deve ser valido."));

        if (command.VeiculoId.HasValue && command.VeiculoId.Value == Guid.Empty)
            return Result<ListarOrdensServicoResponse>.Failure(new Error("O identificador do veiculo deve ser valido."));

        var ordensServicoResult = await _ordemServicoRepository.GetPagedAsync(
            command.ClienteId,
            command.VeiculoId,
            command.Status,
            command.StatusSortDirection,
            command.DataAberturaSortDirection,
            command.PageNumber,
            command.PageSize,
            cancellationToken);

        if (!ordensServicoResult.IsSuccess)
            return Result<ListarOrdensServicoResponse>.Failure(ordensServicoResult.Error);

        return Result<ListarOrdensServicoResponse>.Success(new ListarOrdensServicoResponse
        {
            PageNumber = command.PageNumber,
            PageSize = command.PageSize,
            TotalItems = ordensServicoResult.Value.TotalItems,
            OrdensServico = ordensServicoResult.Value.OrdensServico.Select(ToItemResponse).ToArray()
        });
    }

    private static Result<bool> ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber <= 0)
            return Result<bool>.Failure(new Error("O numero da pagina deve ser maior que zero."));

        if (pageSize <= 0)
            return Result<bool>.Failure(new Error("O tamanho da pagina deve ser maior que zero."));

        return Result<bool>.Success(true);
    }

    private static ListarOrdemServicoItemResponse ToItemResponse(OrdemServico ordemServico)
    {
        return new ListarOrdemServicoItemResponse
        {
            Id = ordemServico.Id,
            ClienteId = ordemServico.ClienteId,
            VeiculoId = ordemServico.VeiculoId,
            DescricaoProblema = ordemServico.DescricaoProblema,
            Status = ordemServico.Status,
            DataCriacao = ordemServico.DataCriacao,
            DataInicioDiagnostico = ordemServico.DataInicioDiagnostico,
            DataEnvioAprovacao = ordemServico.DataEnvioAprovacao,
            DataInicioExecucao = ordemServico.DataInicioExecucao,
            DataFinalizacao = ordemServico.DataFinalizacao,
            DataEntrega = ordemServico.DataEntrega
        };
    }
}
