using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.CriarVeiculo;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VeiculosController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CriarVeiculoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post([FromServices] ICriarVeiculoUseCase useCase, [FromBody] CriarVeiculoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error.Description.Contains("Cliente", StringComparison.OrdinalIgnoreCase)
                && result.Error.Description.Contains("encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = result.Error.Description });

            return BadRequest(new { error = result.Error.Description });
        }

        return CreatedAtAction(nameof(Post), new { id = result.Value!.Id }, result.Value);
    }
}
