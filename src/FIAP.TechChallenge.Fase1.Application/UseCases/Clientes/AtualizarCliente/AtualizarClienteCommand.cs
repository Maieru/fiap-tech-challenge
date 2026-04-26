namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.AtualizarCliente;

public sealed class AtualizarClienteCommand
{
    [Description("Identificador do cliente. Em atualizacoes pela API, este valor e preenchido pela rota.")]
    public Guid Id { get; init; }

    [Required]
    [StringLength(150, MinimumLength = 3)]
    [Description("Nome completo ou razao social do cliente.")]
    public string Nome { get; init; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 10)]
    [RegularExpression(@"^\d{10,11}$")]
    [Description("Telefone com DDD, somente numeros. Use 10 digitos para fixo ou 11 para celular.")]
    public string Telefone { get; init; } = string.Empty;

    [EmailAddress]
    [StringLength(200)]
    [Description("E-mail do cliente. Campo opcional.")]
    public string? Email { get; init; }
}
