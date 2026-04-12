using FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.CadastrarServico;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ServicosController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CadastrarServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post([FromServices] ICadastrarServicoUseCase useCase, [FromBody] CadastrarServicoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error.Description });

        return CreatedAtAction(nameof(Post), new { id = result.Value!.Id }, result.Value);
    }
}
