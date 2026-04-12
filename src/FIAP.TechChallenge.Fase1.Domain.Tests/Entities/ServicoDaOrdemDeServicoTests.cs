using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Tests.Entities;

[TestFixture]
internal sealed class ServicoDaOrdemDeServicoTests
{
    [Test]
    public void Create_ShouldFail_WhenServicoIsNull()
    {
        var result = ServicoDaOrdemDeServico.Create(Guid.NewGuid(), null!, 1);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O serviço da ordem de serviço é obrigatório."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenOrdemServicoIdIsEmpty()
    {
        var servico = CreateServicoValido();

        var result = ServicoDaOrdemDeServico.Create(Guid.Empty, servico, 1);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O serviço deve estar associado a uma ordem de serviço válida."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenQuantidadeIsZero()
    {
        var servico = CreateServicoValido();

        var result = ServicoDaOrdemDeServico.Create(Guid.NewGuid(), servico, 0);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A quantidade do serviço da ordem de serviço deve ser maior que zero."));
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValid()
    {
        var ordemServicoId = Guid.NewGuid();
        var servico = CreateServicoValido();

        var result = ServicoDaOrdemDeServico.Create(ordemServicoId, servico, 3);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
        });

        var entity = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(entity.OrdemServicoId, Is.EqualTo(ordemServicoId));
            Assert.That(entity.ServicoId, Is.EqualTo(servico.Id));
            Assert.That(entity.Descricao, Is.EqualTo("Troca de óleo"));
            Assert.That(entity.ValorUnitario, Is.EqualTo(150m));
            Assert.That(entity.Quantidade, Is.EqualTo(3));
            Assert.That(entity.ValorTotal, Is.EqualTo(450m));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenIdIsEmpty()
    {
        var result = ServicoDaOrdemDeServico.Rehydrate(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Serviço",
            10m,
            1);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O id do serviço da ordem de serviço é inválido."));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenServicoIdIsEmpty()
    {
        var result = ServicoDaOrdemDeServico.Rehydrate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            "Serviço",
            10m,
            1);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O serviço informado é inválido."));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenDescricaoIsWhitespace()
    {
        var result = ServicoDaOrdemDeServico.Rehydrate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "   ",
            10m,
            1);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A descrição do serviço da ordem de serviço é obrigatória."));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenDescricaoHasMoreThanOneThousandCharacters()
    {
        var result = ServicoDaOrdemDeServico.Rehydrate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new string('a', 1001),
            10m,
            1);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A descrição do serviço da ordem de serviço deve conter no máximo 1000 caracteres."));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenValorUnitarioIsNegative()
    {
        var result = ServicoDaOrdemDeServico.Rehydrate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Serviço",
            -0.01m,
            1);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O valor unitário do serviço da ordem de serviço não pode ser negativo."));
        });
    }

    [Test]
    public void Rehydrate_ShouldSucceed_WhenInputIsValid()
    {
        var id = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();

        var result = ServicoDaOrdemDeServico.Rehydrate(id, ordemServicoId, servicoId, "  Balanceamento  ", 75.5m, 2);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
        });

        var entity = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(id));
            Assert.That(entity.OrdemServicoId, Is.EqualTo(ordemServicoId));
            Assert.That(entity.ServicoId, Is.EqualTo(servicoId));
            Assert.That(entity.Descricao, Is.EqualTo("Balanceamento"));
            Assert.That(entity.ValorUnitario, Is.EqualTo(75.5m));
            Assert.That(entity.Quantidade, Is.EqualTo(2));
            Assert.That(entity.ValorTotal, Is.EqualTo(151m));
        });
    }

    [Test]
    public void UpdateQuantidade_ShouldFail_WhenQuantidadeIsNegative()
    {
        var entity = CreateServicoDaOrdemDeServicoValido();

        var result = entity.UpdateQuantidade(-1);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("A quantidade do serviço da ordem de serviço deve ser maior que zero."));
            Assert.That(entity.Quantidade, Is.EqualTo(2));
        });
    }

    [Test]
    public void UpdateQuantidade_ShouldSucceed_WhenQuantidadeIsValid()
    {
        var entity = CreateServicoDaOrdemDeServicoValido();

        var result = entity.UpdateQuantidade(5);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(entity.Quantidade, Is.EqualTo(5));
            Assert.That(entity.ValorTotal, Is.EqualTo(500m));
        });
    }

    [Test]
    public void UpdateValorUnitario_ShouldFail_WhenValorUnitarioIsNegative()
    {
        var entity = CreateServicoDaOrdemDeServicoValido();

        var result = entity.UpdateValorUnitario(-1m);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("O valor unitário do serviço da ordem de serviço não pode ser negativo."));
            Assert.That(entity.ValorUnitario, Is.EqualTo(100m));
        });
    }

    [Test]
    public void UpdateValorUnitario_ShouldSucceed_WhenValorUnitarioIsValid()
    {
        var entity = CreateServicoDaOrdemDeServicoValido();

        var result = entity.UpdateValorUnitario(80m);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(entity.ValorUnitario, Is.EqualTo(80m));
            Assert.That(entity.ValorTotal, Is.EqualTo(160m));
        });
    }

    [Test]
    public void UpdateDescricao_ShouldFail_WhenDescricaoIsWhitespace()
    {
        var entity = CreateServicoDaOrdemDeServicoValido();

        var result = entity.UpdateDescricao("   ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("A descrição do serviço da ordem de serviço é obrigatória."));
            Assert.That(entity.Descricao, Is.EqualTo("Serviço Inicial"));
        });
    }

    [Test]
    public void UpdateDescricao_ShouldFail_WhenDescricaoHasMoreThanOneThousandCharacters()
    {
        var entity = CreateServicoDaOrdemDeServicoValido();

        var result = entity.UpdateDescricao(new string('a', 1001));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("A descrição do serviço da ordem de serviço deve conter no máximo 1000 caracteres."));
            Assert.That(entity.Descricao, Is.EqualTo("Serviço Inicial"));
        });
    }

    [Test]
    public void UpdateDescricao_ShouldSucceed_WhenDescricaoIsValid()
    {
        var entity = CreateServicoDaOrdemDeServicoValido();

        var result = entity.UpdateDescricao("  Serviço Atualizado  ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(entity.Descricao, Is.EqualTo("Serviço Atualizado"));
        });
    }

    private static Servico CreateServicoValido()
    {
        var result = Servico.Create("  Troca de óleo  ", 150m);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        });

        return result.Value!;
    }

    private static ServicoDaOrdemDeServico CreateServicoDaOrdemDeServicoValido()
    {
        var result = ServicoDaOrdemDeServico.Rehydrate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Serviço Inicial",
            100m,
            2);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        });

        return result.Value!;
    }
}
