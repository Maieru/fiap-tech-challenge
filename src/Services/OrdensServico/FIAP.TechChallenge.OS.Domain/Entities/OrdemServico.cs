using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Enums;

namespace FIAP.TechChallenge.OS.Domain.Entities;

// Creation invariants extracted from the legacy aggregate. Other lifecycle stages
// belong to the next increment, coordinated with Billing and Execution.
public sealed class OrdemServico
{
    public Guid Id { get; private init; }
    public Guid ClienteId { get; private init; }
    public Guid VeiculoId { get; private init; }
    public string DescricaoProblema { get; private init; } = string.Empty;
    public StatusOrdemServico Status { get; private init; }
    public DateTime DataCriacao { get; private init; }
    public DateTime? DataInicioDiagnostico => null;
    public DateTime? DataEnvioAprovacao => null;
    public DateTime? DataInicioExecucao => null;
    public DateTime? DataFinalizacao => null;
    public DateTime? DataEntrega => null;

    public static Result<OrdemServico> Create(Guid clienteId, Guid veiculoId, string descricaoProblema) =>
        Rehydrate(Guid.NewGuid(), clienteId, veiculoId, descricaoProblema, DateTime.UtcNow);

    public static Result<OrdemServico> Rehydrate(Guid id, Guid clienteId, Guid veiculoId, string descricaoProblema, DateTime dataCriacao)
    {
        if (id == Guid.Empty)
            return Result<OrdemServico>.Failure(new Error("O id da ordem de serviço é inválido."));
        if (clienteId == Guid.Empty)
            return Result<OrdemServico>.Failure(new Error("A ordem de serviço deve estar associada a um cliente válido."));
        if (veiculoId == Guid.Empty)
            return Result<OrdemServico>.Failure(new Error("A ordem de serviço deve estar associada a um veículo válido."));
        if (dataCriacao == default)
            return Result<OrdemServico>.Failure(new Error("A data de criação da ordem de serviço é obrigatória."));
        if (string.IsNullOrWhiteSpace(descricaoProblema))
            return Result<OrdemServico>.Failure(new Error("A descrição do problema é obrigatória."));
        descricaoProblema = descricaoProblema.Trim();
        if (descricaoProblema.Length < 3)
            return Result<OrdemServico>.Failure(new Error("A descrição do problema deve ter pelo menos 3 caracteres."));
        if (descricaoProblema.Length > 1000)
            return Result<OrdemServico>.Failure(new Error("A descrição do problema deve ter no máximo 1000 caracteres."));
        return Result<OrdemServico>.Success(new OrdemServico
        {
            Id = id, ClienteId = clienteId, VeiculoId = veiculoId, DescricaoProblema = descricaoProblema,
            Status = StatusOrdemServico.Recebida, DataCriacao = dataCriacao
        });
    }
}
