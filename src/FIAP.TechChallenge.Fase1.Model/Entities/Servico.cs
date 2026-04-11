using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Domain.Entities;

public sealed class Servico
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public string Descricao { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public int Quantidade { get; private set; }
    public decimal ValorTotal => ValorUnitario * Quantidade;

    private Servico(Guid id, Guid ordemServicoId, string descricao, decimal valorUnitario, int quantidade)
    {
        Id = id;
        OrdemServicoId = ordemServicoId;
        Descricao = descricao;
        ValorUnitario = valorUnitario;
        Quantidade = quantidade;
    }

    public static Result<Servico> Create(Guid ordemServicoId, string descricao, decimal valorUnitario, int quantidade)
    {
        return Create(Guid.NewGuid(), ordemServicoId, descricao, valorUnitario, quantidade);
    }

    public static Result<Servico> Rehydrate(Guid id, Guid ordemServicoId, string descricao, decimal valorUnitario, int quantidade)
    {
        if (id == Guid.Empty)
            return Result<Servico>.Failure(new Error("O id do serviço é inválido."));

        return Create(id, ordemServicoId, descricao, valorUnitario, quantidade);
    }

    private static Result<Servico> Create(Guid id, Guid ordemServicoId, string descricao, decimal valorUnitario, int quantidade)
    {
        if (ordemServicoId == Guid.Empty)
            return Result<Servico>.Failure(new Error("A ordem de serviço informada é inválida."));

        if (string.IsNullOrWhiteSpace(descricao))
            return Result<Servico>.Failure(new Error("A descrição do serviço é obrigatória."));

        descricao = descricao.Trim();

        if (valorUnitario < 0)
            return Result<Servico>.Failure(new Error("O valor do serviço não pode ser negativo."));

        if (quantidade <= 0)
            return Result<Servico>.Failure(new Error("A quantidade do serviço deve ser maior que zero."));

        var item = new Servico(Guid.NewGuid(), ordemServicoId, descricao, valorUnitario, quantidade);
        return Result<Servico>.Success(item);
    }
}