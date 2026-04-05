using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Domain.Tests.Entities;

[TestFixture]
internal class ClienteTests
{
    [Test]
    public void Create_ShouldFail_WhenNameIsNull()
    {
        var telefone = CreateTelefoneValido();

        var result = Cliente.Create(null!, CreateCpfValido(), null, telefone, null);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O nome do cliente é obrigatório."));
    }

    [Test]
    public void Create_ShouldFail_WhenNameIsWhitespace()
    {
        var telefone = CreateTelefoneValido();

        var result = Cliente.Create("   ", CreateCpfValido(), null, telefone, null);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O nome do cliente é obrigatório."));
    }

    [Test]
    public void Create_ShouldFail_WhenNameHasLessThanThreeCharacters()
    {
        var telefone = CreateTelefoneValido();

        var result = Cliente.Create("Ab", CreateCpfValido(), null, telefone, null);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O nome do cliente deve ter pelo menos 3 caracteres."));
    }

    [Test]
    public void Create_ShouldFail_WhenNameHasMoreThanOneHundredAndFiftyCharacters()
    {
        var telefone = CreateTelefoneValido();
        var nome = new string('a', 151);

        var result = Cliente.Create(nome, CreateCpfValido(), null, telefone, null);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O nome do cliente deve ter no máximo 150 caracteres."));
    }

    [Test]
    public void Create_ShouldFail_WhenCpfAndCnpjAreNull()
    {
        var telefone = CreateTelefoneValido();

        var result = Cliente.Create("Cliente Teste", null, null, telefone, null);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O cliente deve possuir CPF ou CNPJ."));
    }

    [Test]
    public void Create_ShouldFail_WhenCpfAndCnpjAreInformed()
    {
        var telefone = CreateTelefoneValido();

        var result = Cliente.Create("Cliente Teste", CreateCpfValido(), CreateCnpjValido(), telefone, null);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O cliente não pode possuir CPF e CNPJ ao mesmo tempo."));
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValidForPessoaFisica()
    {
        var telefone = CreateTelefoneValido();
        var email = CreateEmailValido();

        var result = Cliente.Create("  Cliente Teste  ", CreateCpfValido(), null, telefone, email);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Error, Is.EqualTo(Abstractions.Error.None));
        Assert.That(result.Value, Is.Not.Null);

        var cliente = result.Value!;

        Assert.That(cliente.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(cliente.Nome, Is.EqualTo("Cliente Teste"));
        Assert.That(cliente.Cpf, Is.Not.Null);
        Assert.That(cliente.Cnpj, Is.Null);
        Assert.That(cliente.Telefone, Is.EqualTo(telefone));
        Assert.That(cliente.Email, Is.EqualTo(email));
        Assert.That(cliente.TipoPessoa, Is.EqualTo(TipoPessoa.Fisica));
        Assert.That(cliente.GetDocumentoFormatado(), Is.EqualTo(cliente.Cpf!.Formatted));
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValidForPessoaJuridica()
    {
        var telefone = CreateTelefoneValido();

        var result = Cliente.Create("Empresa Teste", null, CreateCnpjValido(), telefone, null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);

        var cliente = result.Value!;

        Assert.That(cliente.Cpf, Is.Null);
        Assert.That(cliente.Cnpj, Is.Not.Null);
        Assert.That(cliente.TipoPessoa, Is.EqualTo(TipoPessoa.Juridica));
        Assert.That(cliente.GetDocumentoFormatado(), Is.EqualTo(cliente.Cnpj!.Formatted));
    }

    [Test]
    public void UpdateName_ShouldFail_WhenNameIsWhitespace()
    {
        var cliente = CreateClienteFisico();

        var result = cliente.UpdateName("   ");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.False);
        Assert.That(result.Error.Description, Is.EqualTo("O nome do cliente é obrigatório."));
        Assert.That(cliente.Nome, Is.EqualTo("Cliente Inicial"));
    }

    [Test]
    public void UpdateName_ShouldFail_WhenNameHasLessThanThreeCharacters()
    {
        var cliente = CreateClienteFisico();

        var result = cliente.UpdateName("Ab");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.False);
        Assert.That(result.Error.Description, Is.EqualTo("O nome do cliente deve ter pelo menos 3 caracteres."));
        Assert.That(cliente.Nome, Is.EqualTo("Cliente Inicial"));
    }

    [Test]
    public void UpdateName_ShouldSucceed_WhenNameIsValid()
    {
        var cliente = CreateClienteFisico();

        var result = cliente.UpdateName("  Nome Atualizado  ");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
        Assert.That(result.Error, Is.EqualTo(Abstractions.Error.None));
        Assert.That(cliente.Nome, Is.EqualTo("Nome Atualizado"));
    }

    [Test]
    public void UpdateEmail_ShouldSucceed_WhenChangingEmail()
    {
        var cliente = CreateClienteFisico();
        var novoEmail = CreateEmailValido("novo.email@exemplo.com");

        var result = cliente.UpdateEmail(novoEmail);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
        Assert.That(cliente.Email, Is.EqualTo(novoEmail));
    }

    [Test]
    public void UpdateEmail_ShouldSucceed_WhenSettingNullEmail()
    {
        var cliente = CreateClienteFisico();

        var result = cliente.UpdateEmail(null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
        Assert.That(cliente.Email, Is.Null);
    }

    [Test]
    public void UpdateTelefone_ShouldSucceed_WhenChangingTelefone()
    {
        var cliente = CreateClienteFisico();
        var novoTelefone = CreateTelefoneValido("1132654321");

        var result = cliente.UpdateTelefone(novoTelefone);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
        Assert.That(cliente.Telefone, Is.EqualTo(novoTelefone));
    }

    private static Cliente CreateClienteFisico()
    {
        var telefone = CreateTelefoneValido();
        var email = CreateEmailValido();

        var clienteResult = Cliente.Create("Cliente Inicial", CreateCpfValido(), null, telefone, email);

        Assert.That(clienteResult.IsSuccess, Is.True);
        Assert.That(clienteResult.Value, Is.Not.Null);

        return clienteResult.Value!;
    }

    private static Cpf CreateCpfValido()
    {
        var result = Cpf.Create("52998224725");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);

        return result.Value!;
    }

    private static Cnpj CreateCnpjValido()
    {
        var result = Cnpj.Create("11444777000161");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);

        return result.Value!;
    }

    private static Telefone CreateTelefoneValido(string value = "11987654321")
    {
        var result = Telefone.Create(value);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);

        return result.Value!;
    }

    private static Email CreateEmailValido(string value = "cliente@exemplo.com")
    {
        var result = Email.Create(value);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);

        return result.Value!;
    }
}