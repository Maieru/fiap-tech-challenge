using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;

public sealed class CriarClienteUseCase(IClienteRepository clienteRepository) : ICriarClienteUseCase
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async Task<Result<CriarClienteResponse>> ExecuteAsync(CriarClienteCommand command, CancellationToken cancellationToken = default)
    {
        var telefoneResult = CreateTelefone(command.Telefone);
        if (!telefoneResult.IsSuccess || telefoneResult.Value is null)
            return Result<CriarClienteResponse>.Failure(telefoneResult.Error);

        var cpfResult = await CreateCpfAsync(command.Cpf, cancellationToken);
        if (!cpfResult.IsSuccess)
            return Result<CriarClienteResponse>.Failure(cpfResult.Error);

        var cnpjResult = await CreateCnpjAsync(command.Cnpj, cancellationToken);
        if (!cnpjResult.IsSuccess)
            return Result<CriarClienteResponse>.Failure(cnpjResult.Error);

        var emailResult = CreateEmail(command.Email);
        if (!emailResult.IsSuccess)
            return Result<CriarClienteResponse>.Failure(emailResult.Error);

        var clienteResult = Cliente.Create(command.Nome, cpfResult.Value, cnpjResult.Value, telefoneResult.Value, emailResult.Value);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<CriarClienteResponse>.Failure(clienteResult.Error);

        var cliente = clienteResult.Value;

        await _clienteRepository.AddAsync(cliente, cancellationToken);

        return Result<CriarClienteResponse>.Success(new CriarClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Cpf = cliente.Cpf?.Formatted,
            Cnpj = cliente.Cnpj?.Formatted,
            Telefone = cliente.Telefone.Formatted,
            Email = cliente.Email?.Value
        });
    }

    private static Result<Telefone> CreateTelefone(string telefone)
    {
        var telefoneResult = Telefone.Create(telefone);
        return !telefoneResult.IsSuccess || telefoneResult.Value is null
            ? Result<Telefone>.Failure(telefoneResult.Error)
            : Result<Telefone>.Success(telefoneResult.Value);
    }

    private async Task<Result<Cpf?>> CreateCpfAsync(string? cpf, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return Result<Cpf?>.Success(null);

        var cpfResult = Cpf.Create(cpf);
        if (!cpfResult.IsSuccess || cpfResult.Value is null)
            return Result<Cpf?>.Failure(cpfResult.Error);

        var cpfJaExiste = await _clienteRepository.ExistsByCpfAsync(cpfResult.Value.Unformatted, cancellationToken);
        if (cpfJaExiste)
            return Result<Cpf?>.Failure(new Error("Já existe um cliente cadastrado com este CPF."));

        return Result<Cpf?>.Success(cpfResult.Value);
    }

    private async Task<Result<Cnpj?>> CreateCnpjAsync(string? cnpj, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return Result<Cnpj?>.Success(null);

        var cnpjResult = Cnpj.Create(cnpj);
        if (!cnpjResult.IsSuccess || cnpjResult.Value is null)
            return Result<Cnpj?>.Failure(cnpjResult.Error);

        var cnpjJaExiste = await _clienteRepository.ExistsByCnpjAsync(cnpjResult.Value.Unformatted, cancellationToken);
        if (cnpjJaExiste)
            return Result<Cnpj?>.Failure(new Error("Já existe um cliente cadastrado com este CNPJ."));

        return Result<Cnpj?>.Success(cnpjResult.Value);
    }

    private static Result<Email?> CreateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email?>.Success(null);

        var emailResult = Email.Create(email);
        return !emailResult.IsSuccess || emailResult.Value is null
            ? Result<Email?>.Failure(emailResult.Error)
            : Result<Email?>.Success(emailResult.Value);
    }
}