using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FIAP.TechChallenge.OS.Domain.Entities;

public sealed record OrdemServicoSnapshot(Guid Id, Guid ClienteId, Guid VeiculoId, string DescricaoProblema, DateTime DataCriacao,
    StatusOrdemServico Status, DateTime? DataInicioDiagnostico, DateTime? DataEnvioAprovacao, DateTime? DataInicioExecucao,
    DateTime? DataFinalizacao, string? AcaoPendente,
    Guid? SolicitacaoPendenteId, string? DiagnosticoJson);

public sealed class OrdemServico
{
    public Guid Id { get; private init; }
    public Guid ClienteId { get; private init; }
    public Guid VeiculoId { get; private init; }
    public string DescricaoProblema { get; private init; } = string.Empty;
    public StatusOrdemServico Status { get; private set; }
    public DateTime DataCriacao { get; private init; }
    public DateTime? DataInicioDiagnostico { get; private set; }
    public DateTime? DataEnvioAprovacao { get; private set; }
    public DateTime? DataInicioExecucao { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }
    public DateTime? DataEntrega => null;
    public string? AcaoPendente { get; private set; }
    public Guid? SolicitacaoPendenteId { get; private set; }
    [JsonIgnore] public string? DiagnosticoJson { get; private set; }
    public JsonElement? Diagnostico => DiagnosticoJson is null ? null : JsonDocument.Parse(DiagnosticoJson).RootElement.Clone();

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
            Id = id,
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            DescricaoProblema = descricaoProblema,
            Status = StatusOrdemServico.Recebida,
            DataCriacao = dataCriacao
        });
    }

    public static Result<OrdemServico> Rehydrate(OrdemServicoSnapshot snapshot)
    {
        var result = Rehydrate(snapshot.Id, snapshot.ClienteId, snapshot.VeiculoId, snapshot.DescricaoProblema, snapshot.DataCriacao);

        if (!result.IsSuccess)
            return result;

        if (!Enum.IsDefined(snapshot.Status))
            return Result<OrdemServico>.Failure(new Error("Estado persistido da ordem de servico invalido."));

        var entity = result.Value!;
        entity.Status = snapshot.Status;
        entity.DataInicioDiagnostico = snapshot.DataInicioDiagnostico;
        entity.DataEnvioAprovacao = snapshot.DataEnvioAprovacao;
        entity.DataInicioExecucao = snapshot.DataInicioExecucao;
        entity.DataFinalizacao = snapshot.DataFinalizacao;
        entity.AcaoPendente = snapshot.AcaoPendente;
        entity.SolicitacaoPendenteId = snapshot.SolicitacaoPendenteId;
        entity.DiagnosticoJson = snapshot.DiagnosticoJson;
        return result;
    }

    public Result<Guid> Solicitar(string acao)
    {
        if (Status != StatusOrdemServico.AguardandoAprovacao)
            return Result<Guid>.Failure(new Error("Somente ordens aguardando aprovacao permitem esta solicitacao.", ErrorCode.Conflict));

        if (AcaoPendente is not null)
            return Result<Guid>.Failure(new Error("Ja existe uma solicitacao pendente para esta ordem.", ErrorCode.Conflict));

        if (acao is not ("Autorizacao" or "Cancelamento"))
            return Result<Guid>.Failure(new Error("Acao invalida."));

        AcaoPendente = acao;
        SolicitacaoPendenteId = Guid.NewGuid();
        return Result<Guid>.Success(SolicitacaoPendenteId.Value);
    }

    public Result<bool> AplicarEvento(string type, DateTime occurred, Guid? requestId, string? diagnosticoJson)
    {
        var valid = type switch
        {
            "DiagnosticoIniciado" => Status == StatusOrdemServico.Recebida,
            "DiagnosticoConcluido" => Status == StatusOrdemServico.EmDiagnostico && diagnosticoJson is not null,
            "ExecucaoIniciada" => Status == StatusOrdemServico.AguardandoAprovacao && AcaoPendente == "Autorizacao" && requestId == SolicitacaoPendenteId,
            "ExecucaoCancelada" => Status == StatusOrdemServico.AguardandoAprovacao && AcaoPendente == "Cancelamento" && requestId == SolicitacaoPendenteId,
            "ExecucaoFinalizada" => Status == StatusOrdemServico.EmExecucao,
            "SolicitacaoRecusada" => AcaoPendente is not null && requestId == SolicitacaoPendenteId,
            _ => false
        };

        if (!valid)
            return Result<bool>.Failure(new Error("Evento incompativel com o estado atual da ordem.", ErrorCode.Conflict));

        switch (type)
        {
            case "DiagnosticoIniciado":
                Status = StatusOrdemServico.EmDiagnostico;
                DataInicioDiagnostico = occurred;
                break;
            case "DiagnosticoConcluido":
                Status = StatusOrdemServico.AguardandoAprovacao;
                DataEnvioAprovacao = occurred;
                DiagnosticoJson = diagnosticoJson;
                break;
            case "ExecucaoIniciada":
                Status = StatusOrdemServico.EmExecucao;
                DataInicioExecucao = occurred;
                break;
            case "ExecucaoFinalizada":
                Status = StatusOrdemServico.Finalizada;
                DataFinalizacao = occurred;
                break;
            case "ExecucaoCancelada":
                Status = StatusOrdemServico.Cancelada;
                break;
        }
        if (type is "ExecucaoIniciada" or "ExecucaoCancelada" or "SolicitacaoRecusada")
        { AcaoPendente = null; SolicitacaoPendenteId = null; }
        return Result<bool>.Success(true);
    }
}
