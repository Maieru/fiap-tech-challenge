using FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ClienteEntity> Clientes => Set<ClienteEntity>();
    public DbSet<VeiculoEntity> Veiculos => Set<VeiculoEntity>();
    public DbSet<OrdemServicoEntity> OrdensServico => Set<OrdemServicoEntity>();
    public DbSet<HistoricoOrdemEntity> HistoricoOrdens => Set<HistoricoOrdemEntity>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        var clientes = model.Entity<ClienteEntity>();
        clientes.HasQueryFilter(x => x.Ativo);
        clientes.Property(x => x.Ativo).HasDefaultValue(true).IsConcurrencyToken();
        clientes.HasIndex(x => x.Cpf).IsUnique().HasFilter("\"Ativo\" = true AND \"Cpf\" IS NOT NULL");
        clientes.HasIndex(x => x.Cnpj).IsUnique().HasFilter("\"Ativo\" = true AND \"Cnpj\" IS NOT NULL");
        clientes.ToTable("Clientes", table => table.HasCheckConstraint("CK_Clientes_Documento", "(\"Cpf\" IS NOT NULL) <> (\"Cnpj\" IS NOT NULL)"));

        var veiculos = model.Entity<VeiculoEntity>();
        veiculos.HasQueryFilter(x => x.Ativo);
        veiculos.Property(x => x.Ativo).HasDefaultValue(true).IsConcurrencyToken();
        veiculos.HasIndex(x => x.Placa).IsUnique().HasFilter("\"Ativo\" = true");
        veiculos.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);

        var ordens = model.Entity<OrdemServicoEntity>();
        ordens.Property(x => x.DescricaoProblema).HasMaxLength(1000).IsRequired();
        ordens.HasIndex(x => new { x.DataCriacao, x.Id });
        // No required navigation to a filtered customer/vehicle: archived orders remain readable.
        ordens.HasOne<ClienteEntity>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        ordens.HasOne<VeiculoEntity>().WithMany().HasForeignKey(x => x.VeiculoId).OnDelete(DeleteBehavior.Restrict);
        ordens.ToTable("OrdensServico", table =>
        {
            table.HasCheckConstraint("CK_Ordens_Descricao", "length(btrim(\"DescricaoProblema\")) >= 3");
            table.HasCheckConstraint("CK_Ordens_Status", "\"Status\" = 1");
        });

        var historico = model.Entity<HistoricoOrdemEntity>();
        historico.Property(x => x.Tipo).HasMaxLength(100).IsRequired();
        historico.Property(x => x.CorrelationId).HasMaxLength(128);
        historico.HasIndex(x => new { x.OrdemServicoId, x.OcorridoEmUtc, x.Id });
        historico.HasOne<OrdemServicoEntity>().WithMany().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Restrict);
    }
}
