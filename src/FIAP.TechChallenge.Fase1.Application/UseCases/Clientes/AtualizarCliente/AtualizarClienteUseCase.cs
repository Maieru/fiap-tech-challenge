using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.AtualizarCliente;

public sealed class AtualizarClienteUseCase(IClienteRepository clienteRepository) : IAtualizarClienteUseCase
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;

    public async Task<Result<AtualizarClienteResponse>> ExecuteAsync(AtualizarClienteCommand command, CancellationToken cancellationToken = default)
    {
        var clienteResult = await _clienteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (!clienteResult.IsSuccess || clienteResult.Value is null)
            return Result<AtualizarClienteResponse>.Failure(clienteResult.Error);

        var telefoneResult = CreateTelefone(command.Telefone);

        if (!telefoneResult.IsSuccess || telefoneResult.Value is null)
            return Result<AtualizarClienteResponse>.Failure(telefoneResult.Error);

        var emailResult = CreateEmail(command.Email);

        if (!emailResult.IsSuccess)
            return Result<AtualizarClienteResponse>.Failure(emailResult.Error);

        var cliente = clienteResult.Value;

        var updateNameResult = cliente.UpdateName(command.Nome);

        if (!updateNameResult.IsSuccess)
            return Result<AtualizarClienteResponse>.Failure(updateNameResult.Error);

        var updateTelefoneResult = cliente.UpdateTelefone(telefoneResult.Value);

        if (!updateTelefoneResult.IsSuccess)
            return Result<AtualizarClienteResponse>.Failure(updateTelefoneResult.Error);

        var updateEmailResult = cliente.UpdateEmail(emailResult.Value);

        if (!updateEmailResult.IsSuccess)
            return Result<AtualizarClienteResponse>.Failure(updateEmailResult.Error);

        await _clienteRepository.UpdateAsync(cliente, cancellationToken);

        return Result<AtualizarClienteResponse>.Success(new AtualizarClienteResponse
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

