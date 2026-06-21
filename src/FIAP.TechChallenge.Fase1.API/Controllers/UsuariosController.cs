using FIAP.TechChallenge.Fase1.API.Extensions;
using FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.AutenticarUsuario;
using FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.CriarUsuario;
using FIAP.TechChallenge.Fase1.Application.UseCases.Usuarios.ExcluirUsuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsuariosController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AutenticarUsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        IAutenticarUsuarioUseCase useCase,
        AutenticarUsuarioCommand command,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(CriarUsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Post(ICriarUsuarioUseCase useCase, CriarUsuarioCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(Post), new { id = value.Id }, value));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, IExcluirUsuarioUseCase useCase, CancellationToken cancellationToken)
    {
        var command = new ExcluirUsuarioCommand { Id = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, _ => NoContent());
    }
}

