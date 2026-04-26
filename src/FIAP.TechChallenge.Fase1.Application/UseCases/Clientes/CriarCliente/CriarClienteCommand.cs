namespace FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;

public sealed class CriarClienteCommand
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    [Description("Nome completo ou razao social do cliente.")]
    public string Nome { get; init; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 10)]
    [RegularExpression(@"^\d{10,11}$")]
    [Description("Telefone com DDD, somente numeros. Use 10 digitos para fixo ou 11 para celular.")]
    public string Telefone { get; init; } = string.Empty;

    [StringLength(14, MinimumLength = 11)]
    [Description("CPF do cliente, somente numeros ou no formato 000.000.000-00. Informe CPF ou CNPJ, nunca ambos.")]
    public string? Cpf { get; init; }

    [StringLength(18, MinimumLength = 14)]
    [Description("CNPJ do cliente, alfanumerico com 14 caracteres ou no formato 00.000.000/0000-00. Informe CPF ou CNPJ, nunca ambos.")]
    public string? Cnpj { get; init; }

    [EmailAddress]
    [StringLength(200)]
    [Description("E-mail do cliente. Campo opcional.")]
    public string? Email { get; init; }
}
