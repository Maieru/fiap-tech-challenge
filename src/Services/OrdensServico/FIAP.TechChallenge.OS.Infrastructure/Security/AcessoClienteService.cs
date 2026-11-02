using System.Security.Cryptography;
using System.Text;
using FIAP.TechChallenge.OS.Application.Abstractions;
using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
namespace FIAP.TechChallenge.OS.Infrastructure.Security;

public sealed class AcessoClienteService(AppDbContext db) : IAcessoClienteService
{
    public async Task<string> EmitirAsync(Guid ordemServicoId, CancellationToken cancellationToken)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var record = await db.AcessosCliente.SingleOrDefaultAsync(x => x.OrdemServicoId == ordemServicoId, cancellationToken);
        if (record is null) { record = new() { OrdemServicoId = ordemServicoId }; db.AcessosCliente.Add(record); }
        record.TokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        await db.SaveChangesAsync(cancellationToken);
        return token;
    }
    public async Task<bool> ValidarAsync(Guid ordemServicoId, string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token) || token.Length != 64 || !token.All(char.IsAsciiHexDigit)) return false;
        var hash = await db.AcessosCliente.AsNoTracking().Where(x => x.OrdemServicoId == ordemServicoId).Select(x => x.TokenHash).SingleOrDefaultAsync(cancellationToken);
        if (hash is null || hash.Length != 64 || !hash.All(char.IsAsciiHexDigit)) return false;
        return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(hash), SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
