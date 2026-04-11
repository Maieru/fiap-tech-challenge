using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

public static class ClienteMapper
{
    public static ClienteEntity ToEntity(Cliente cliente)
    {
        return new ClienteEntity
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Cpf = cliente.Cpf?.Unformatted,
            Cnpj = cliente.Cnpj?.Unformatted,
            Telefone = cliente.Telefone.Unformatted,
            Email = cliente.Email?.Value
        };
    }

    public static Result<Cliente> ToDomain(ClienteEntity entity)
    {
        var telefoneResult = Telefone.Create(entity.Telefone);

        if (!telefoneResult.IsSuccess)
            return Result<Cliente>.Failure(telefoneResult.Error);

        Cpf? cpf = null;
        Cnpj? cnpj = null;
        Email? email = null;

        if (!string.IsNullOrWhiteSpace(entity.Cpf))
        {
            var cpfResult = Cpf.Create(entity.Cpf);

            if (!cpfResult.IsSuccess)
                return Result<Cliente>.Failure(cpfResult.Error);

            cpf = cpfResult.Value;
        }

        if (!string.IsNullOrWhiteSpace(entity.Cnpj))
        {
            var cnpjResult = Cnpj.Create(entity.Cnpj);

            if (!cnpjResult.IsSuccess)
                return Result<Cliente>.Failure(cnpjResult.Error);

            cnpj = cnpjResult.Value;
        }

        if (!string.IsNullOrWhiteSpace(entity.Email))
        {
            var emailResult = Email.Create(entity.Email);
            if (!emailResult.IsSuccess)
                return Result<Cliente>.Failure(emailResult.Error);

            email = emailResult.Value;
        }

        return Cliente.Rehydrate(entity.Id, entity.Nome, cpf, cnpj, telefoneResult.Value!, email);
    }
}