namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.CriarUsuario;

public sealed class CriarUsuarioCommand
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Description("Login do usuario.")]
    public string Usuario { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Description("Senha do usuario. Deve possuir pelo menos 8 caracteres.")]
    public string Senha { get; init; } = string.Empty;
}
