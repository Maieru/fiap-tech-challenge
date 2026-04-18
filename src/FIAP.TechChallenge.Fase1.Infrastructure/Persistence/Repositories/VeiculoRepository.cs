using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class VeiculoRepository(AppDbContext context) : IVeiculoRepository
{
    public async Task<bool> ExistsByPlacaAsync(string placa, CancellationToken cancellationToken = default)
    {
        return await context.Veiculos.AnyAsync(x => x.Placa == placa, cancellationToken);
    }

    public async Task<Result<Veiculo>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var veiculo = await context.Veiculos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (veiculo == null)
            return Result<Veiculo>.Failure(new Error("Veiculo não encontrado.", ErrorCode.NotFound));

        return VeiculoMapper.ToDomain(veiculo);
    }

    public async Task<Result<Veiculo>> GetByPlacaAsync(string placa, CancellationToken cancellationToken = default)
    {
        var veiculo = await context.Veiculos.AsNoTracking().FirstOrDefaultAsync(x => x.Placa == placa, cancellationToken);

        if (veiculo == null)
            return Result<Veiculo>.Failure(new Error("Veiculo não encontrado.", ErrorCode.NotFound));

        return VeiculoMapper.ToDomain(veiculo);
    }

    public async Task<Result<(IReadOnlyCollection<Veiculo> Veiculos, int TotalItems)>> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        var totalItems = await context.Veiculos.CountAsync(x => x.ClienteId == clienteId, cancellationToken);

        var veiculosEntity = await context.Veiculos
            .AsNoTracking()
            .Where(x => x.ClienteId == clienteId)
            .OrderBy(x => x.Placa)
            .ToListAsync(cancellationToken);

        var veiculosResult = MapToDomainCollection(veiculosEntity);

        if (!veiculosResult.IsSuccess || veiculosResult.Value is null)
            return Result<(IReadOnlyCollection<Veiculo> Veiculos, int TotalItems)>.Failure(veiculosResult.Error);

        return Result<(IReadOnlyCollection<Veiculo> Veiculos, int TotalItems)>.Success((veiculosResult.Value, totalItems));
    }

    public async Task<Result<(IReadOnlyCollection<Veiculo> Veiculos, int TotalItems)>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalItems = await context.Veiculos.CountAsync(cancellationToken);

        var veiculosEntity = await context.Veiculos
            .AsNoTracking()
            .OrderBy(x => x.Placa)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var veiculosResult = MapToDomainCollection(veiculosEntity);

        if (!veiculosResult.IsSuccess || veiculosResult.Value is null)
            return Result<(IReadOnlyCollection<Veiculo> Veiculos, int TotalItems)>.Failure(veiculosResult.Error);

        return Result<(IReadOnlyCollection<Veiculo> Veiculos, int TotalItems)>.Success((veiculosResult.Value, totalItems));
    }

    public async Task AddAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        var veiculoEntity = VeiculoMapper.ToEntity(veiculo);
        _ = await context.Veiculos.AddAsync(veiculoEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        var veiculoEntity = VeiculoMapper.ToEntity(veiculo);
        _ = context.Veiculos.Update(veiculoEntity);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    private static Result<List<Veiculo>> MapToDomainCollection(List<VeiculoEntity> veiculosEntity)
    {
        var veiculos = new List<Veiculo>(veiculosEntity.Count);

        foreach (var entity in veiculosEntity)
        {
            var veiculoResult = VeiculoMapper.ToDomain(entity);

            if (!veiculoResult.IsSuccess || veiculoResult.Value is null)
                return Result<List<Veiculo>>.Failure(veiculoResult.Error);

            veiculos.Add(veiculoResult.Value);
        }

        return Result<List<Veiculo>>.Success(veiculos);
    }
}
