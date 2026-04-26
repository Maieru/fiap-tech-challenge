namespace FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.ExcluirUsuario;

public sealed class ExcluirUsuarioCommand
{
    [Description("Identificador do usuario a excluir.")]
    public Guid Id { get; set; }
}
