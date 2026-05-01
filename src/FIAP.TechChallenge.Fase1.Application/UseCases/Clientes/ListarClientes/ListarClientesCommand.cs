namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ListarClientes;

public sealed class ListarClientesCommand
{
    public string? Cpf { get; init; }
    public string? Cnpj { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
