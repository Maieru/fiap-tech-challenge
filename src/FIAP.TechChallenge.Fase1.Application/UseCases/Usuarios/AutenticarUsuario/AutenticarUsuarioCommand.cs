namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.AutenticarUsuario;

public sealed class AutenticarUsuarioCommand
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Description("Login do usuario.")]
    public string Usuario { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Description("Senha do usuario.")]
    public string Senha { get; init; } = string.Empty;
}
