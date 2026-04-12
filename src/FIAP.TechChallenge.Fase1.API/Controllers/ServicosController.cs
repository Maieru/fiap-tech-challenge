using FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.CadastrarServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.AtualizarServico;
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AtualizarServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromServices] IAtualizarServicoUseCase useCase, [FromBody] AtualizarServicoCommand command, CancellationToken cancellationToken)
    {
        var updateCommand = new AtualizarServicoCommand
        {
            Id = id,
            Descricao = command.Descricao,
            ValorUnitario = command.ValorUnitario
        };

        var result = await useCase.ExecuteAsync(updateCommand, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error.Description.Contains("Servico", StringComparison.OrdinalIgnoreCase)
                && result.Error.Description.Contains("encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = result.Error.Description });

            return BadRequest(new { error = result.Error.Description });
        }

        return Ok(result.Value);
    }
}
