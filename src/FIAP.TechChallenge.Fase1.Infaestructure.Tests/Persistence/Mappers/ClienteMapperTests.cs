using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Mappers;

[TestFixture]
internal sealed class ClienteMapperTests
{
    [Test]
    public void ToEntity_ShouldMapAllFields_WhenClienteHasCpfAndEmail()
    {
        var cliente = CreateClienteComCpf();

        var entity = ClienteMapper.ToEntity(cliente);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(cliente.Id));
            Assert.That(entity.Nome, Is.EqualTo("Cliente Teste"));
            Assert.That(entity.Cpf, Is.EqualTo("52998224725"));
            Assert.That(entity.Cnpj, Is.Null);
            Assert.That(entity.Telefone, Is.EqualTo("11987654321"));
            Assert.That(entity.Email, Is.EqualTo("cliente@exemplo.com"));
        });
    }

    [Test]
    public void ToEntity_ShouldMapOptionalFieldsAsNull_WhenClienteHasOnlyCnpj()
    {
        var cliente = CreateClienteComCnpj();

        var entity = ClienteMapper.ToEntity(cliente);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Cpf, Is.Null);
            Assert.That(entity.Cnpj, Is.EqualTo("11444777000161"));
            Assert.That(entity.Email, Is.Null);
        });
    }

    [Test]
    public void ToDomain_ShouldReturnSuccess_WhenEntityIsValidWithCpf()
    {
        var entity = new ClienteEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Mapper",
            Cpf = "52998224725",
            Telefone = "11987654321",
            Email = "mapper@exemplo.com"
        };

        var result = ClienteMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(entity.Id));
            Assert.That(result.Value.Nome, Is.EqualTo(entity.Nome));
            Assert.That(result.Value.Cpf!.Unformatted, Is.EqualTo(entity.Cpf));
            Assert.That(result.Value.Cnpj, Is.Null);
            Assert.That(result.Value.Telefone.Unformatted, Is.EqualTo(entity.Telefone));
            Assert.That(result.Value.Email!.Value, Is.EqualTo(entity.Email));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnFailure_WhenTelefoneIsInvalid()
    {
        var entity = new ClienteEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Mapper",
            Cpf = "52998224725",
            Telefone = "123"
        };

        var result = ClienteMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnFailure_WhenCpfIsInvalid()
    {
        var entity = new ClienteEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Mapper",
            Cpf = "12345678901",
            Telefone = "11987654321"
        };

        var result = ClienteMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnFailure_WhenEmailIsInvalid()
    {
        var entity = new ClienteEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Mapper",
            Cpf = "52998224725",
            Telefone = "11987654321",
            Email = "email-invalido"
        };

        var result = ClienteMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });
    }

    private static Cliente CreateClienteComCpf()
    {
        var cpf = Cpf.Create("52998224725").Value!;
        var telefone = Telefone.Create("11987654321").Value!;
        var email = Email.Create("cliente@exemplo.com").Value;

        return Cliente.Create("Cliente Teste", cpf, null, telefone, email).Value!;
    }

    private static Cliente CreateClienteComCnpj()
    {
        var cnpj = Cnpj.Create("11444777000161").Value!;
        var telefone = Telefone.Create("11987654321").Value!;

        return Cliente.Create("Empresa Teste", null, cnpj, telefone, null).Value!;
    }
}
