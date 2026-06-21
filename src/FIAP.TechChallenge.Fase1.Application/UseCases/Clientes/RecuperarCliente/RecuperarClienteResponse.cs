namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.RecuperarCliente;

public sealed class RecuperarClienteResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string? Cpf { get; init; }
    public string? Cnpj { get; init; }
    public string? Email { get; init; }
}

