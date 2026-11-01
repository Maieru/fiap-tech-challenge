using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Enums;

namespace FIAP.TechChallenge.OS.Domain.Entities;

public sealed class OrdemServico
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public string DescricaoProblema { get; private set; } = string.Empty;
    public StatusOrdemServico Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataInicioDiagnostico { get; private set; }
    public DateTime? DataEnvioAprovacao { get; private set; }
    public DateTime? DataInicioExecucao { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }
    public DateTime? DataEntrega => null;
    public DateTime? DataCancelamento { get; private set; }
    public DecisaoExecucao Decisao { get; private set; }
    public DateTime? DataDecisaoUtc { get; private set; }
    public long Version { get; private set; }

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
        if (!Enum.IsDefined(snapshot.Status) || snapshot.Status == StatusOrdemServico.Entregue
            || !Enum.IsDefined(snapshot.Decisao) || snapshot.Version < 0
            || (snapshot.Decisao != DecisaoExecucao.Nenhuma && snapshot.DataDecisaoUtc is null)
            || (snapshot.Status != StatusOrdemServico.Recebida && snapshot.DataInicioDiagnostico is null)
            || (snapshot.Status is StatusOrdemServico.AguardandoAprovacao or StatusOrdemServico.EmExecucao or StatusOrdemServico.Finalizada or StatusOrdemServico.Cancelada && snapshot.DataEnvioAprovacao is null)
            || (snapshot.Status is StatusOrdemServico.EmExecucao or StatusOrdemServico.Finalizada && (snapshot.DataInicioExecucao is null || snapshot.Decisao != DecisaoExecucao.Autorizada))
            || (snapshot.Status == StatusOrdemServico.Finalizada && snapshot.DataFinalizacao is null)
            || (snapshot.Status == StatusOrdemServico.Cancelada && (snapshot.DataCancelamento is null || snapshot.Decisao != DecisaoExecucao.CancelamentoSolicitado)))
            return Result<OrdemServico>.Failure(new Error("Estado e datas da ordem inconsistentes."));
        var ordem = result.Value!;
        ordem.Status = snapshot.Status;
        ordem.Decisao = snapshot.Decisao;
        ordem.DataDecisaoUtc = snapshot.DataDecisaoUtc;
        ordem.DataInicioDiagnostico = snapshot.DataInicioDiagnostico;
        ordem.DataEnvioAprovacao = snapshot.DataEnvioAprovacao;
        ordem.DataInicioExecucao = snapshot.DataInicioExecucao;
        ordem.DataFinalizacao = snapshot.DataFinalizacao;
        ordem.DataCancelamento = snapshot.DataCancelamento;
        ordem.Version = snapshot.Version;
        return result;
    }

    public OrdemServicoSnapshot Snapshot() => new(Id, ClienteId, VeiculoId, DescricaoProblema, DataCriacao,
        Status, Decisao, DataDecisaoUtc, DataInicioDiagnostico, DataEnvioAprovacao, DataInicioExecucao,
        DataFinalizacao, DataCancelamento, Version);

    public Result<bool> DecidirExecucao(DecisaoExecucao decisao, DateTime ocorridoEmUtc)
    {
        if (decisao is not (DecisaoExecucao.Autorizada or DecisaoExecucao.CancelamentoSolicitado) || ocorridoEmUtc.Kind != DateTimeKind.Utc)
            return Result<bool>.Failure(new Error("Decisão ou data inválida."));
        if (Decisao == decisao)
            return Result<bool>.Success(false);
        if (Decisao != DecisaoExecucao.Nenhuma)
            return Result<bool>.Failure(Error.Conflict("A ordem já possui uma decisão diferente."));
        if (Status != StatusOrdemServico.AguardandoAprovacao)
            return Result<bool>.Failure(new Error("A ordem precisa estar aguardando aprovação."));
        Decisao = decisao;
        DataDecisaoUtc = ocorridoEmUtc;
        return Result<bool>.Success(true);
    }
    public Result<bool> RegistrarDiagnosticoIniciado(DateTime ocorridoEmUtc)
    {
        if (ocorridoEmUtc == default || ocorridoEmUtc.Kind != DateTimeKind.Utc)
            return Result<bool>.Failure(new Error("Data da transição inválida."));
        if (DataInicioDiagnostico.HasValue)
            return Result<bool>.Success(false);
        if (Status != StatusOrdemServico.Recebida)
            return Result<bool>.Failure(new Error("Transição DiagnosticoIniciado incompatível com o estado ou decisão atual."));
        Status = StatusOrdemServico.EmDiagnostico;
        DataInicioDiagnostico = ocorridoEmUtc;
        return Result<bool>.Success(true);
    }
    public Result<bool> RegistrarDiagnosticoConcluido(DateTime ocorridoEmUtc)
    {
        if (ocorridoEmUtc == default || ocorridoEmUtc.Kind != DateTimeKind.Utc)
            return Result<bool>.Failure(new Error("Data da transição inválida."));
        if (DataEnvioAprovacao.HasValue)
            return Result<bool>.Success(false);
        if (Status != StatusOrdemServico.EmDiagnostico)
            return Result<bool>.Failure(new Error("Transição DiagnosticoConcluido incompatível com o estado ou decisão atual."));
        Status = StatusOrdemServico.AguardandoAprovacao;
        DataEnvioAprovacao = ocorridoEmUtc;
        return Result<bool>.Success(true);
    }
    public Result<bool> RegistrarExecucaoIniciada(DateTime ocorridoEmUtc)
    {
        if (ocorridoEmUtc == default || ocorridoEmUtc.Kind != DateTimeKind.Utc)
            return Result<bool>.Failure(new Error("Data da transição inválida."));
        if (DataInicioExecucao.HasValue)
            return Result<bool>.Success(false);
        if (Status != StatusOrdemServico.AguardandoAprovacao || Decisao != DecisaoExecucao.Autorizada)
            return Result<bool>.Failure(new Error("Transição ExecucaoIniciada incompatível com o estado ou decisão atual."));
        Status = StatusOrdemServico.EmExecucao;
        DataInicioExecucao = ocorridoEmUtc;
        return Result<bool>.Success(true);
    }
    public Result<bool> RegistrarExecucaoConcluida(DateTime ocorridoEmUtc)
    {
        if (ocorridoEmUtc == default || ocorridoEmUtc.Kind != DateTimeKind.Utc)
            return Result<bool>.Failure(new Error("Data da transição inválida."));
        if (DataFinalizacao.HasValue)
            return Result<bool>.Success(false);
        if (Status != StatusOrdemServico.EmExecucao || Decisao != DecisaoExecucao.Autorizada)
            return Result<bool>.Failure(new Error("Transição ExecucaoConcluida incompatível com o estado ou decisão atual."));
        Status = StatusOrdemServico.Finalizada;
        DataFinalizacao = ocorridoEmUtc;
        return Result<bool>.Success(true);
    }
    public Result<bool> RegistrarExecucaoCancelada(DateTime ocorridoEmUtc)
    {
        if (ocorridoEmUtc == default || ocorridoEmUtc.Kind != DateTimeKind.Utc)
            return Result<bool>.Failure(new Error("Data da transição inválida."));
        if (DataCancelamento.HasValue)
            return Result<bool>.Success(false);
        if (Status != StatusOrdemServico.AguardandoAprovacao || Decisao != DecisaoExecucao.CancelamentoSolicitado)
            return Result<bool>.Failure(new Error("Transição ExecucaoCancelada incompatível com o estado ou decisão atual."));
        Status = StatusOrdemServico.Cancelada;
        DataCancelamento = ocorridoEmUtc;
        return Result<bool>.Success(true);
    }
}
