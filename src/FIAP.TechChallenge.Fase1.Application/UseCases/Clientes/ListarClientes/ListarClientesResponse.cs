namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ListarClientes;

public sealed class ListarClientesResponse
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public IReadOnlyCollection<ListarClienteItemResponse> Clientes { get; init; } = [];
}

public sealed class ListarClienteItemResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string? Cpf { get; init; }
    public string? Cnpj { get; init; }
    public string? Email { get; init; }
}

