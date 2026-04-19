using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Domain.Entities;

/// <summary>
/// Representa um serviço efetivamente adicionado a uma ordem de serviço,
/// preservando os dados usados no momento de adição.
/// </summary>
public sealed class ServicoDaOrdemDeServico
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public Guid ServicoId { get; private set; }

    public string Descricao { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public int Quantidade { get; private set; }
    public int? TempoGastoMinutos { get; private set; }
    public bool Concluido { get; private set; }

    public decimal ValorTotal => ValorUnitario * Quantidade;

    private ServicoDaOrdemDeServico(
        Guid id,
        Guid ordemServicoId,
        Guid servicoId,
        string descricao,
        decimal valorUnitario,
        int quantidade,
        int? tempoGastoMinutos,
        bool concluido)
    {
        Id = id;
        OrdemServicoId = ordemServicoId;
        ServicoId = servicoId;
        Descricao = descricao;
        ValorUnitario = valorUnitario;
        Quantidade = quantidade;
        TempoGastoMinutos = tempoGastoMinutos;
        Concluido = concluido;
    }

    public static Result<ServicoDaOrdemDeServico> Create(Guid ordemServicoId, Servico servico, int quantidade)
    {
        if (servico is null)
            return Result<ServicoDaOrdemDeServico>.Failure(new Error("O serviço da ordem de serviço é obrigatório."));

        return Create(Guid.NewGuid(), ordemServicoId, servico.Id, servico.Descricao, servico.ValorUnitario, quantidade, null, false);
    }

    public static Result<ServicoDaOrdemDeServico> Rehydrate(
        Guid id,
        Guid ordemServicoId,
        Guid servicoId,
        string descricao,
        decimal valorUnitario,
        int quantidade,
        int? tempoGastoMinutos,
        bool concluido)
    {
        if (id == Guid.Empty)
            return Result<ServicoDaOrdemDeServico>.Failure(new Error("O id do serviço da ordem de serviço é inválido."));

        return Create(id, ordemServicoId, servicoId, descricao, valorUnitario, quantidade, tempoGastoMinutos, concluido);
    }

    private static Result<ServicoDaOrdemDeServico> Create(
        Guid id,
        Guid ordemServicoId,
        Guid servicoId,
        string descricao,
        decimal valorUnitario,
        int quantidade,
        int? tempoGastoMinutos,
        bool concluido)
    {
        if (ordemServicoId == Guid.Empty)
            return Result<ServicoDaOrdemDeServico>.Failure(new Error("O serviço deve estar associado a uma ordem de serviço válida."));

        if (servicoId == Guid.Empty)
            return Result<ServicoDaOrdemDeServico>.Failure(new Error("O serviço informado é inválido."));

        if (string.IsNullOrWhiteSpace(descricao))
            return Result<ServicoDaOrdemDeServico>.Failure(new Error("A descrição do serviço da ordem de serviço é obrigatória."));

        descricao = descricao.Trim();

        if (descricao.Length > 1000)
            return Result<ServicoDaOrdemDeServico>.Failure(new Error("A descrição do serviço da ordem de serviço deve conter no máximo 1000 caracteres."));

        if (valorUnitario < 0)
            return Result<ServicoDaOrdemDeServico>.Failure(new Error("O valor unitário do serviço da ordem de serviço não pode ser negativo."));

        if (quantidade <= 0)
            return Result<ServicoDaOrdemDeServico>.Failure(new Error("A quantidade do serviço da ordem de serviço deve ser maior que zero."));

        if (concluido && (!tempoGastoMinutos.HasValue || tempoGastoMinutos.Value <= 0))
            return Result<ServicoDaOrdemDeServico>.Failure(new Error("O tempo gasto do serviço concluído da ordem de serviço deve ser maior que zero."));

        if (!concluido && tempoGastoMinutos.HasValue)
            return Result<ServicoDaOrdemDeServico>.Failure(new Error("Não é permitido informar tempo gasto para serviço não concluído da ordem de serviço."));

        var entity = new ServicoDaOrdemDeServico(id, ordemServicoId, servicoId, descricao, valorUnitario, quantidade, tempoGastoMinutos, concluido);

        return Result<ServicoDaOrdemDeServico>.Success(entity);
    }

    public Result<bool> Concluir(int tempoGastoMinutos)
    {
        if (Concluido)
            return Result<bool>.Failure(new Error("O serviço da ordem de serviço já foi concluído."));

        if (tempoGastoMinutos <= 0)
            return Result<bool>.Failure(new Error("O tempo gasto do serviço da ordem de serviço deve ser maior que zero."));

        TempoGastoMinutos = tempoGastoMinutos;
        Concluido = true;

        return Result<bool>.Success(true);
    }

    public Result<bool> UpdateQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            return Result<bool>.Failure(new Error("A quantidade do serviço da ordem de serviço deve ser maior que zero."));

        Quantidade = quantidade;
        return Result<bool>.Success(true);
    }

    public Result<bool> UpdateValorUnitario(decimal valorUnitario)
    {
        if (valorUnitario < 0)
            return Result<bool>.Failure(new Error("O valor unitário do serviço da ordem de serviço não pode ser negativo."));

        ValorUnitario = valorUnitario;
        return Result<bool>.Success(true);
    }

    public Result<bool> UpdateDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return Result<bool>.Failure(new Error("A descrição do serviço da ordem de serviço é obrigatória."));

        descricao = descricao.Trim();

        if (descricao.Length > 1000)
            return Result<bool>.Failure(new Error("A descrição do serviço da ordem de serviço deve conter no máximo 1000 caracteres."));

        Descricao = descricao;
        return Result<bool>.Success(true);
    }
}
