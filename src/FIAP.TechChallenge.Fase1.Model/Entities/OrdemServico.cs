using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Domain.Entities;

public sealed record OrdemServicoSnapshot(
    Guid Id,
    Guid ClienteId,
    Guid VeiculoId,
    string DescricaoProblema,
    StatusOrdemServico Status,
    DateTime DataCriacao,
    DateTime? DataInicioDiagnostico,
    DateTime? DataEnvioAprovacao,
    DateTime? DataInicioExecucao,
    DateTime? DataFinalizacao,
    DateTime? DataEntrega);

public sealed class OrdemServico
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }

    public string DescricaoProblema { get; private set; }
    public StatusOrdemServico Status { get; private set; }

    public DateTime DataCriacao { get; private set; }
    public DateTime? DataInicioDiagnostico { get; private set; }
    public DateTime? DataEnvioAprovacao { get; private set; }
    public DateTime? DataInicioExecucao { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }
    public DateTime? DataEntrega { get; private set; }

    private OrdemServico(Guid id, Guid clienteId, Guid veiculoId, string descricaoProblema, StatusOrdemServico status, DateTime dataCriacao)
    {
        Id = id;
        ClienteId = clienteId;
        VeiculoId = veiculoId;
        DescricaoProblema = descricaoProblema;
        Status = status;
        DataCriacao = dataCriacao;
    }

    public static Result<OrdemServico> Create(Guid clienteId, Guid veiculoId, string descricaoProblema)
    {
        return Create(Guid.NewGuid(), clienteId, veiculoId, descricaoProblema, StatusOrdemServico.Recebida, DateTime.UtcNow);
    }

    public static Result<OrdemServico> Rehydrate(OrdemServicoSnapshot snapshot)
    {
        if (snapshot.Id == Guid.Empty)
            return Result<OrdemServico>.Failure(new Error("O id da ordem de serviço é inválido."));

        var validacaoConsistenciaFluxoResult = IsStatusValid(snapshot.Status, snapshot.DataInicioDiagnostico, snapshot.DataEnvioAprovacao, snapshot.DataInicioExecucao, snapshot.DataFinalizacao, snapshot.DataEntrega);

        if (!validacaoConsistenciaFluxoResult.IsSuccess)
            return Result<OrdemServico>.Failure(validacaoConsistenciaFluxoResult.Error);

        var ordemServicoResult = Create(snapshot.Id, snapshot.ClienteId, snapshot.VeiculoId, snapshot.DescricaoProblema, snapshot.Status, snapshot.DataCriacao);

        if (!ordemServicoResult.IsSuccess)
            return Result<OrdemServico>.Failure(ordemServicoResult.Error);

        var ordemServico = ordemServicoResult.Value!;

        ordemServico.DataInicioDiagnostico = snapshot.DataInicioDiagnostico;
        ordemServico.DataEnvioAprovacao = snapshot.DataEnvioAprovacao;
        ordemServico.DataInicioExecucao = snapshot.DataInicioExecucao;
        ordemServico.DataFinalizacao = snapshot.DataFinalizacao;
        ordemServico.DataEntrega = snapshot.DataEntrega;

        return Result<OrdemServico>.Success(ordemServico);
    }

    private static Result<OrdemServico> Create(Guid id, Guid clienteId, Guid veiculoId, string descricaoProblema, StatusOrdemServico status, DateTime dataCriacao)
    {
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

        var ordemServico = new OrdemServico(id, clienteId, veiculoId, descricaoProblema, status, dataCriacao);

        return Result<OrdemServico>.Success(ordemServico);
    }

    public Result<bool> IniciarDiagnostico()
    {
        if (Status != StatusOrdemServico.Recebida)
            return Result<bool>.Failure(new Error("Somente ordens de serviço recebidas podem iniciar diagnóstico."));

        Status = StatusOrdemServico.EmDiagnostico;
        DataInicioDiagnostico = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    public Result<bool> AguardarAprovacao()
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
            return Result<bool>.Failure(new Error("Somente ordens de serviço em diagnóstico podem aguardar aprovação."));

        Status = StatusOrdemServico.AguardandoAprovacao;
        DataEnvioAprovacao = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    public Result<bool> AprovarOrcamento()
    {
        if (Status != StatusOrdemServico.AguardandoAprovacao)
            return Result<bool>.Failure(new Error("Somente ordens de serviço aguardando aprovação podem ser aprovadas."));

        Status = StatusOrdemServico.EmExecucao;
        DataInicioExecucao = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    public Result<bool> Finalizar()
    {
        if (Status != StatusOrdemServico.EmExecucao)
            return Result<bool>.Failure(new Error("Somente ordens de serviço em execução podem ser finalizadas."));

        Status = StatusOrdemServico.Finalizada;
        DataFinalizacao = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    public Result<bool> Entregar()
    {
        if (Status != StatusOrdemServico.Finalizada)
            return Result<bool>.Failure(new Error("Somente ordens de serviço finalizadas podem ser entregues."));

        Status = StatusOrdemServico.Entregue;
        DataEntrega = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    public Result<bool> ValidarAdicaoServico()
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
            return Result<bool>.Failure(new Error("Somente ordens de servico em diagnostico podem receber servicos."));

        return Result<bool>.Success(true);
    }

    private static Result<bool> IsStatusValid(StatusOrdemServico status, DateTime? dataInicioDiagnostico, DateTime? dataEnvioAprovacao, DateTime? dataInicioExecucao, DateTime? dataFinalizacao, DateTime? dataEntrega)
    {
        if (status >= StatusOrdemServico.EmDiagnostico && dataInicioDiagnostico is null)
            return Result<bool>.Failure(new Error("A ordem de serviço em diagnóstico ou posterior deve possuir data de início do diagnóstico."));

        if (status >= StatusOrdemServico.AguardandoAprovacao && dataEnvioAprovacao is null)
            return Result<bool>.Failure(new Error("A ordem de serviço aguardando aprovação ou posterior deve possuir data de envio para aprovação."));

        if (status >= StatusOrdemServico.EmExecucao && dataInicioExecucao is null)
            return Result<bool>.Failure(new Error("A ordem de serviço em execução ou posterior deve possuir data de início da execução."));

        if (status >= StatusOrdemServico.Finalizada && dataFinalizacao is null)
            return Result<bool>.Failure(new Error("A ordem de serviço finalizada ou entregue deve possuir data de finalização."));

        if (status == StatusOrdemServico.Entregue && dataEntrega is null)
            return Result<bool>.Failure(new Error("A ordem de serviço entregue deve possuir data de entrega."));

        return Result<bool>.Success(true);
    }
}
