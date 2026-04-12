using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Domain.Entities;

/// <summary>
/// Representa um serviço do catálogo administrativo.
/// Não pertence diretamente a uma ordem de serviço.
/// </summary>
public sealed class Servico
{
    public Guid Id { get; private set; }
    public string Descricao { get; private set; }
    public decimal ValorUnitario { get; private set; }

    private Servico(Guid id, string descricao, decimal valorUnitario)
    {
        Id = id;
        Descricao = descricao;
        ValorUnitario = valorUnitario;
    }

    public static Result<Servico> Create(string descricao, decimal valorUnitario)
    {
        return Create(Guid.NewGuid(), descricao, valorUnitario);
    }

    public static Result<Servico> Rehydrate(Guid id, string descricao, decimal valorUnitario)
    {
        if (id == Guid.Empty)
            return Result<Servico>.Failure(new Error("O id do serviço é inválido."));

        return Create(id, descricao, valorUnitario);
    }

    private static Result<Servico> Create(Guid id, string descricao, decimal valorUnitario)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return Result<Servico>.Failure(new Error("A descrição do serviço é obrigatória."));

        descricao = descricao.Trim();

        if (valorUnitario < 0)
            return Result<Servico>.Failure(new Error("O valor do serviço não pode ser negativo."));

        if (descricao.Length > 1000)
            return Result<Servico>.Failure(new Error("A descrição do serviço deve conter no máximo 1000 caracteres."));

        var item = new Servico(id, descricao, valorUnitario);
        return Result<Servico>.Success(item);
    }
}