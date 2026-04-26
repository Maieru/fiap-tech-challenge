namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.RecuperarCliente;

public sealed class RecuperarClienteCommand
{
    [Description("Identificador do cliente.")]
    public Guid ClienteId { get; init; }
}
