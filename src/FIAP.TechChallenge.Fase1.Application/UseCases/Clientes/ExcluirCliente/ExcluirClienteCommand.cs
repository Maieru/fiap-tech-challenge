namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ExcluirCliente;

public sealed class ExcluirClienteCommand
{
    [Description("Identificador do cliente a ser excluido.")]
    public Guid Id { get; set; }
}
