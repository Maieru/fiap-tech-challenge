using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Domain.Entities;

public sealed class Veiculo
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public Placa Placa { get; private set; }
    public string Marca { get; private set; }
    public string Modelo { get; private set; }
    public int Ano { get; private set; }

    private Veiculo(Guid id, Guid clienteId, Placa placa, string marca, string modelo, int ano)
    {
        Id = id;
        ClienteId = clienteId;
        Placa = placa;
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
    }

    public static Result<Veiculo> Create(Guid clienteId, Placa placa, string marca, string modelo, int ano)
    {
        return Create(Guid.NewGuid(), clienteId, placa, marca, modelo, ano);
    }

    public static Result<Veiculo> Rehydrate(Guid id, Guid clienteId, Placa placa, string marca, string modelo, int ano)
    {
        if (id == Guid.Empty)
            return Result<Veiculo>.Failure(new Error("O id do Veiculo é inválido."));

        return Create(id, clienteId, placa, marca, modelo, ano);
    }

    private static Result<Veiculo> Create(Guid id, Guid clienteId, Placa placa, string marca, string modelo, int ano)
    {
        if (clienteId == Guid.Empty)
            return Result<Veiculo>.Failure(new Error("O Veiculo deve estar associado a um cliente válido."));

        if (placa is null)
            return Result<Veiculo>.Failure(new Error("A placa do Veiculo é obrigatória."));

        if (string.IsNullOrWhiteSpace(marca))
            return Result<Veiculo>.Failure(new Error("A marca do Veiculo é obrigatória."));

        if (string.IsNullOrWhiteSpace(modelo))
            return Result<Veiculo>.Failure(new Error("O modelo do Veiculo é obrigatório."));

        marca = marca.Trim();
        modelo = modelo.Trim();

        var marcaValidationResult = IsMarcaValid(marca);

        if (!marcaValidationResult.IsSuccess)
            return Result<Veiculo>.Failure(marcaValidationResult.Error);

        var modeloValidationResult = IsModeloValid(modelo);

        if (!modeloValidationResult.IsSuccess)
            return Result<Veiculo>.Failure(modeloValidationResult.Error);

        var anoValidationResult = IsAnoValid(ano);

        if (!anoValidationResult.IsSuccess)
            return Result<Veiculo>.Failure(anoValidationResult.Error);

        var veiculo = new Veiculo(id, clienteId, placa, marca, modelo, ano);

        return Result<Veiculo>.Success(veiculo);
    }

    public Result<bool> UpdateMarca(string marca)
    {
        if (string.IsNullOrWhiteSpace(marca))
            return Result<bool>.Failure(new Error("A marca do Veiculo é obrigatória."));

        marca = marca.Trim();

        var marcaValidationResult = IsMarcaValid(marca);

        if (!marcaValidationResult.IsSuccess)
            return Result<bool>.Failure(marcaValidationResult.Error);

        Marca = marca;
        return Result<bool>.Success(true);
    }

    public Result<bool> UpdateModelo(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
            return Result<bool>.Failure(new Error("O modelo do Veiculo é obrigatório."));

        modelo = modelo.Trim();

        var modeloValidationResult = IsModeloValid(modelo);

        if (!modeloValidationResult.IsSuccess)
            return Result<bool>.Failure(modeloValidationResult.Error);

        Modelo = modelo;
        return Result<bool>.Success(true);
    }

    public Result<bool> UpdateAno(int ano)
    {
        var anoValidationResult = IsAnoValid(ano);

        if (!anoValidationResult.IsSuccess)
            return Result<bool>.Failure(anoValidationResult.Error);

        Ano = ano;
        return Result<bool>.Success(true);
    }

    public Result<bool> UpdatePlaca(Placa placa)
    {
        if (placa is null)
            return Result<bool>.Failure(new Error("A placa do Veiculo é obrigatória."));

        Placa = placa;
        return Result<bool>.Success(true);
    }

    private static Result<bool> IsMarcaValid(string marca)
    {
        if (marca.Length < 2)
            return Result<bool>.Failure(new Error("A marca do Veiculo deve ter pelo menos 2 caracteres."));

        if (marca.Length > 100)
            return Result<bool>.Failure(new Error("A marca do Veiculo deve ter no máximo 100 caracteres."));

        return Result<bool>.Success(true);
    }

    private static Result<bool> IsModeloValid(string modelo)
    {
        if (modelo.Length < 2)
            return Result<bool>.Failure(new Error("O modelo do Veiculo deve ter pelo menos 2 caracteres."));

        if (modelo.Length > 100)
            return Result<bool>.Failure(new Error("O modelo do Veiculo deve ter no máximo 100 caracteres."));

        return Result<bool>.Success(true);
    }

    private static Result<bool> IsAnoValid(int ano)
    {
        var anoAtual = DateTime.UtcNow.Year + 1;

        if (ano < DateTime.MinValue.Year)
            return Result<bool>.Failure(new Error("O ano do Veiculo é inválido."));

        if (ano > anoAtual)
            return Result<bool>.Failure(new Error("O ano do Veiculo não pode ser maior que o próximo ano."));

        return Result<bool>.Success(true);
    }
}