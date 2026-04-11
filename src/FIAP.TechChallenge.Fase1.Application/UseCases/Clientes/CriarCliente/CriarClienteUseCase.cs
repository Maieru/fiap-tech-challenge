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
        var cpfInformado = !string.IsNullOrWhiteSpace(command.Cpf);
        var cnpjInformado = !string.IsNullOrWhiteSpace(command.Cnpj);
        var telefoneResult = Telefone.Create(command.Telefone);

        if (!telefoneResult.IsSuccess || telefoneResult.Value is null)
            return Result<CriarClienteResponse>.Failure(telefoneResult.Error);

        Cpf? cpf = null;
        Cnpj? cnpj = null;
        Email? email = null;

        if (cpfInformado)
        {
            var cpfResult = Cpf.Create(command.Cpf!);

            if (!cpfResult.IsSuccess || cpfResult.Value is null)
                return Result<CriarClienteResponse>.Failure(cpfResult.Error);

            var cpfJaExiste = await _clienteRepository.ExistsByCpfAsync(cpfResult.Value.Unformatted, cancellationToken);

            if (cpfJaExiste)
                return Result<CriarClienteResponse>.Failure(new Error("Já existe um cliente cadastrado com este CPF."));

            cpf = cpfResult.Value;
        }

        if (cnpjInformado)
        {
            var cnpjResult = Cnpj.Create(command.Cnpj!);

            if (!cnpjResult.IsSuccess || cnpjResult.Value is null)
                return Result<CriarClienteResponse>.Failure(cnpjResult.Error);

            var cnpjJaExiste = await _clienteRepository.ExistsByCnpjAsync(cnpjResult.Value.Unformatted, cancellationToken);

            if (cnpjJaExiste)
                return Result<CriarClienteResponse>.Failure(new Error("Já existe um cliente cadastrado com este CNPJ."));

            cnpj = cnpjResult.Value;
        }

        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            var emailResult = Email.Create(command.Email);

            if (!emailResult.IsSuccess || emailResult.Value is null)
                return Result<CriarClienteResponse>.Failure(emailResult.Error);

            email = emailResult.Value;
        }

        var clienteResult = Cliente.Create(command.Nome, cpf, cnpj, telefoneResult.Value, email);

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
}