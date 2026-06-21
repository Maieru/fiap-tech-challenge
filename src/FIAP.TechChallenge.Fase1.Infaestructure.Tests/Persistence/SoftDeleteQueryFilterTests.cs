using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence;

[TestFixture]
internal sealed class SoftDeleteQueryFilterTests
{
    [Test]
    public async Task QueryFilters_ShouldReturnOnlyActiveRows()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        SeedEntities(context);
        _ = await context.SaveChangesAsync();

        var activeClientes = await context.Clientes.CountAsync();
        var activeVeiculos = await context.Veiculos.CountAsync();
        var activeOrdensServico = await context.OrdensServico.CountAsync();
        var activePecasInsumos = await context.PecasInsumos.CountAsync();
        var activeServicos = await context.Servicos.CountAsync();
        var activeUsuarios = await context.Usuarios.CountAsync();
        var activeServicosDaOrdem = await context.ServicoDaOrdemDeServico.CountAsync();
        var activePecasDaOrdem = await context.PecaOuInsumoDaOrdemDeServico.CountAsync();

        var allClientes = await context.Clientes.IgnoreQueryFilters().CountAsync();
        var allVeiculos = await context.Veiculos.IgnoreQueryFilters().CountAsync();
        var allOrdensServico = await context.OrdensServico.IgnoreQueryFilters().CountAsync();
        var allPecasInsumos = await context.PecasInsumos.IgnoreQueryFilters().CountAsync();
        var allServicos = await context.Servicos.IgnoreQueryFilters().CountAsync();
        var allUsuarios = await context.Usuarios.IgnoreQueryFilters().CountAsync();
        var allServicosDaOrdem = await context.ServicoDaOrdemDeServico.IgnoreQueryFilters().CountAsync();
        var allPecasDaOrdem = await context.PecaOuInsumoDaOrdemDeServico.IgnoreQueryFilters().CountAsync();

        Assert.Multiple(() =>
        {
            Assert.That(activeClientes, Is.EqualTo(1));
            Assert.That(activeVeiculos, Is.EqualTo(1));
            Assert.That(activeOrdensServico, Is.EqualTo(1));
            Assert.That(activePecasInsumos, Is.EqualTo(1));
            Assert.That(activeServicos, Is.EqualTo(1));
            Assert.That(activeUsuarios, Is.EqualTo(1));
            Assert.That(activeServicosDaOrdem, Is.EqualTo(1));
            Assert.That(activePecasDaOrdem, Is.EqualTo(1));
            Assert.That(allClientes, Is.EqualTo(2));
            Assert.That(allVeiculos, Is.EqualTo(2));
            Assert.That(allOrdensServico, Is.EqualTo(2));
            Assert.That(allPecasInsumos, Is.EqualTo(2));
            Assert.That(allServicos, Is.EqualTo(2));
            Assert.That(allUsuarios, Is.EqualTo(2));
            Assert.That(allServicosDaOrdem, Is.EqualTo(2));
            Assert.That(allPecasDaOrdem, Is.EqualTo(2));
        });
    }

    private static AppDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedEntities(AppDbContext context)
    {
        var clienteAtivoId = Guid.NewGuid();
        var clienteInativoId = Guid.NewGuid();
        var veiculoAtivoId = Guid.NewGuid();
        var veiculoInativoId = Guid.NewGuid();
        var ordemAtivaId = Guid.NewGuid();
        var ordemInativaId = Guid.NewGuid();

        context.Clientes.AddRange(
            new ClienteEntity { Id = clienteAtivoId, Nome = "Cliente Ativo", Cpf = "52998224725", Telefone = "11987654321", Ativo = true },
            new ClienteEntity { Id = clienteInativoId, Nome = "Cliente Inativo", Cpf = "39053344705", Telefone = "11987654322", Ativo = false });

        context.Veiculos.AddRange(
            new VeiculoEntity { Id = veiculoAtivoId, ClienteId = clienteAtivoId, Placa = "ABC1234", Marca = "Toyota", Modelo = "Corolla", Ano = 2024, Ativo = true },
            new VeiculoEntity { Id = veiculoInativoId, ClienteId = clienteAtivoId, Placa = "DEF5678", Marca = "Honda", Modelo = "Civic", Ano = 2023, Ativo = false });

        context.OrdensServico.AddRange(
            new OrdemServicoEntity { Id = ordemAtivaId, ClienteId = clienteAtivoId, VeiculoId = veiculoAtivoId, DescricaoProblema = "Falha ativa", Status = StatusOrdemServico.Recebida, DataCriacao = DateTime.UtcNow, Ativo = true },
            new OrdemServicoEntity { Id = ordemInativaId, ClienteId = clienteAtivoId, VeiculoId = veiculoAtivoId, DescricaoProblema = "Falha inativa", Status = StatusOrdemServico.Recebida, DataCriacao = DateTime.UtcNow, Ativo = false });

        context.PecasInsumos.AddRange(
            new PecaInsumoEntity { Id = Guid.NewGuid(), Nome = "Peca Ativa", Codigo = "PEC-1", PrecoUnitario = 10m, QuantidadeEstoque = 5, Ativo = true },
            new PecaInsumoEntity { Id = Guid.NewGuid(), Nome = "Peca Inativa", Codigo = "PEC-0", PrecoUnitario = 10m, QuantidadeEstoque = 5, Ativo = false });

        context.Servicos.AddRange(
            new ServicoEntity { Id = Guid.NewGuid(), Descricao = "Servico ativo", ValorUnitario = 100m, Ativo = true },
            new ServicoEntity { Id = Guid.NewGuid(), Descricao = "Servico inativo", ValorUnitario = 100m, Ativo = false });

        context.Usuarios.AddRange(
            new UsuarioEntity { Id = Guid.NewGuid(), Login = "ativo", Senha = "hash", Ativo = true },
            new UsuarioEntity { Id = Guid.NewGuid(), Login = "inativo", Senha = "hash", Ativo = false });

        context.ServicoDaOrdemDeServico.AddRange(
            new ServicoDaOrdemDeServicoEntity { Id = Guid.NewGuid(), OrdemServicoId = ordemAtivaId, ServicoId = Guid.NewGuid(), Descricao = "Servico OS ativo", ValorUnitario = 50m, Quantidade = 1, Concluido = false, Ativo = true },
            new ServicoDaOrdemDeServicoEntity { Id = Guid.NewGuid(), OrdemServicoId = ordemAtivaId, ServicoId = Guid.NewGuid(), Descricao = "Servico OS inativo", ValorUnitario = 50m, Quantidade = 1, Concluido = false, Ativo = false });

        context.PecaOuInsumoDaOrdemDeServico.AddRange(
            new PecaOuInsumoDaOrdemDeServicoEntity { Id = Guid.NewGuid(), OrdemServicoId = ordemAtivaId, PecaInsumoId = Guid.NewGuid(), Nome = "Peca OS ativa", Codigo = "POS-1", PrecoUnitario = 15m, Quantidade = 1, Ativo = true },
            new PecaOuInsumoDaOrdemDeServicoEntity { Id = Guid.NewGuid(), OrdemServicoId = ordemAtivaId, PecaInsumoId = Guid.NewGuid(), Nome = "Peca OS inativa", Codigo = "POS-0", PrecoUnitario = 15m, Quantidade = 1, Ativo = false });
    }
}

