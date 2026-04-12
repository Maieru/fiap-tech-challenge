using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.IncluirPecaInsumo;

public sealed class IncluirPecaInsumoUseCase(IPecaInsumoRepository pecaInsumoRepository) : IIncluirPecaInsumoUseCase
{
    private readonly IPecaInsumoRepository _pecaInsumoRepository = pecaInsumoRepository;

    public async Task<Result<IncluirPecaInsumoResponse>> ExecuteAsync(IncluirPecaInsumoCommand command, CancellationToken cancellationToken = default)
    {
        var pecaInsumoResult = PecaInsumo.Create(command.Nome, command.Codigo, command.Descricao, command.PrecoUnitario, command.QuantidadeEstoque);

        if (!pecaInsumoResult.IsSuccess || pecaInsumoResult.Value is null)
            return Result<IncluirPecaInsumoResponse>.Failure(pecaInsumoResult.Error);

        var codigoJaExiste = await _pecaInsumoRepository.ExistsByCodigoAsync(pecaInsumoResult.Value.Codigo, cancellationToken);

        if (codigoJaExiste)
            return Result<IncluirPecaInsumoResponse>.Failure(new Error("Ja existe uma peca ou insumo cadastrado com este codigo."));

        var pecaInsumo = pecaInsumoResult.Value;

        await _pecaInsumoRepository.AddAsync(pecaInsumo, cancellationToken);

        return Result<IncluirPecaInsumoResponse>.Success(new IncluirPecaInsumoResponse
        {
            Id = pecaInsumo.Id,
            Nome = pecaInsumo.Nome,
            Codigo = pecaInsumo.Codigo,
            Descricao = pecaInsumo.Descricao,
            PrecoUnitario = pecaInsumo.PrecoUnitario,
            QuantidadeEstoque = pecaInsumo.QuantidadeEstoque,
            Ativo = pecaInsumo.Ativo
        });
    }
}
