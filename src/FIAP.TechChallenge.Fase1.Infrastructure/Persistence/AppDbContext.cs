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

        modelBuilder.Entity<ClienteEntity>().Property(x => x.Ativo).HasDefaultValue(true);
        modelBuilder.Entity<ClienteEntity>().HasQueryFilter(x => x.Ativo);

        modelBuilder.Entity<VeiculoEntity>().Property(x => x.Ativo).HasDefaultValue(true);
        modelBuilder.Entity<VeiculoEntity>().HasQueryFilter(x => x.Ativo);

        modelBuilder.Entity<OrdemServicoEntity>().Property(x => x.Ativo).HasDefaultValue(true);
        modelBuilder.Entity<OrdemServicoEntity>().HasQueryFilter(x => x.Ativo);

        modelBuilder.Entity<PecaInsumoEntity>().Property(x => x.Ativo).HasDefaultValue(true);
        modelBuilder.Entity<PecaInsumoEntity>().HasQueryFilter(x => x.Ativo);

        modelBuilder.Entity<ServicoEntity>().Property(x => x.Ativo).HasDefaultValue(true);
        modelBuilder.Entity<ServicoEntity>().HasQueryFilter(x => x.Ativo);

        modelBuilder.Entity<UsuarioEntity>().Property(x => x.Ativo).HasDefaultValue(true);
        modelBuilder.Entity<UsuarioEntity>().HasQueryFilter(x => x.Ativo);

        modelBuilder.Entity<ServicoDaOrdemDeServicoEntity>().Property(x => x.Ativo).HasDefaultValue(true);
        modelBuilder.Entity<ServicoDaOrdemDeServicoEntity>().HasQueryFilter(x => x.Ativo);

        modelBuilder.Entity<PecaOuInsumoDaOrdemDeServicoEntity>().Property(x => x.Ativo).HasDefaultValue(true);
        modelBuilder.Entity<PecaOuInsumoDaOrdemDeServicoEntity>().HasQueryFilter(x => x.Ativo);

        base.OnModelCreating(modelBuilder);
    }
}
