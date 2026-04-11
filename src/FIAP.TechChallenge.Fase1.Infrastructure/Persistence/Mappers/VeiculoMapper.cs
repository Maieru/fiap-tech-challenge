using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

public static class VeiculoMapper
{
    public static VeiculoEntity ToEntity(Veiculo veiculo)
    {
        return new VeiculoEntity
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            Placa = veiculo.Placa.Unformatted,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano
        };
    }

    public static Result<Veiculo> ToDomain(VeiculoEntity entity)
    {
        var placaResult = Placa.Create(entity.Placa);

        if (!placaResult.IsSuccess)
            return Result<Veiculo>.Failure(placaResult.Error);

        return Veiculo.Rehydrate(entity.Id, entity.ClienteId, placaResult.Value!, entity.Marca, entity.Modelo, entity.Ano);
    }
}
