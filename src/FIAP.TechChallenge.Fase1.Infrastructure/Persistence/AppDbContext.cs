using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ClienteEntity> Clientes { get; set; }
    public DbSet<VeiculoEntity> Veiculos { get; set; }
    public DbSet<OrdemServicoEntity> OrdensServico { get; set; }
    public DbSet<PecaInsumoEntity> PecasInsumos { get; set; }
    public DbSet<ServicoEntity> Servicos { get; set; }
    public DbSet<UsuarioEntity> Usuarios { get; set; }
    public DbSet<ServicoDaOrdemDeServicoEntity> ServicoDaOrdemDeServico { get; set; }
    public DbSet<PecaOuInsumoDaOrdemDeServicoEntity> PecaOuInsumoDaOrdemDeServico { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
