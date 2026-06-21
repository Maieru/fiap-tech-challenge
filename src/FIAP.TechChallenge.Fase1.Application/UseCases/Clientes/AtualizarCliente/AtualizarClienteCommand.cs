namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.AtualizarCliente;

public sealed class AtualizarClienteCommand
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string? Email { get; init; }
}

