using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Domain.Entities;

public sealed class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public Cpf? Cpf { get; private set; }
    public Cnpj? Cnpj { get; private set; }
    public Telefone Telefone { get; private set; }
    public Email? Email { get; private set; }
    public TipoPessoa TipoPessoa { get; private set; }

    private Cliente(Guid id, string nome, Cpf? cpf, Cnpj? cnpj, Telefone telefone, Email? email)
    {
        Id = id;
        Nome = nome;
        Cpf = cpf;
        Cnpj = cnpj;
        Telefone = telefone;
        Email = email;

        if (Cpf is not null)
            TipoPessoa = TipoPessoa.Fisica;
        else if (Cnpj is not null)
            TipoPessoa = TipoPessoa.Juridica;
    }

    public static Result<Cliente> Create(string nome, Cpf? cpf, Cnpj? cnpj, Telefone telefone, Email? email)
    {
        return Create(Guid.NewGuid(), nome, cpf, cnpj, telefone, email);
    }

    public static Result<Cliente> Rehydrate(Guid id, string nome, Cpf? cpf, Cnpj? cnpj, Telefone telefone, Email? email)
    {
        if (id == Guid.Empty)
            return Result<Cliente>.Failure(new Error("O id do cliente é inválido."));

        return Create(id, nome, cpf, cnpj, telefone, email);
    }

    private static Result<Cliente> Create(Guid id, string nome, Cpf? cpf, Cnpj? cnpj, Telefone telefone, Email? email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result<Cliente>.Failure(new Error("O nome do cliente é obrigatório."));

        nome = nome.Trim();

        var nomeValidationResult = IsNomeValid(nome);

        if (!nomeValidationResult.IsSuccess)
            return Result<Cliente>.Failure(nomeValidationResult.Error);

        if (cpf is null && cnpj is null)
            return Result<Cliente>.Failure(new Error("O cliente deve possuir CPF ou CNPJ."));

        if (cpf is not null && cnpj is not null)
            return Result<Cliente>.Failure(new Error("O cliente não pode possuir CPF e CNPJ ao mesmo tempo."));

        var cliente = new Cliente(id, nome, cpf, cnpj, telefone, email);
        return Result<Cliente>.Success(cliente);
    }

    public Result<bool> UpdateName(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result<bool>.Failure(new Error("O nome do cliente é obrigatório."));

        nome = nome.Trim();

        var nomeValidationResult = IsNomeValid(nome);

        if (!nomeValidationResult.IsSuccess)
            return Result<bool>.Failure(nomeValidationResult.Error);

        Nome = nome;

        return Result<bool>.Success(true);
    }

    public Result<bool> UpdateEmail(Email? email)
    {
        Email = email;
        return Result<bool>.Success(true);
    }

    public Result<bool> UpdateTelefone(Telefone telefone)
    {
        Telefone = telefone;
        return Result<bool>.Success(true);
    }

    public string GetDocumentoFormatado()
    {
        if (Cpf is not null)
            return Cpf.Formatted;

        if (Cnpj is not null)
            return Cnpj.Formatted;

        return string.Empty;
    }

    private static Result<bool> IsNomeValid(string nome)
    {
        if (nome.Length < 3)
            return Result<bool>.Failure(new Error("O nome do cliente deve ter pelo menos 3 caracteres."));

        if (nome.Length > 150)
            return Result<bool>.Failure(new Error("O nome do cliente deve ter no máximo 150 caracteres."));

        return Result<bool>.Success(true);
    }
}