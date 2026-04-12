using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdensServicoController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CriarOrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post([FromServices] ICriarOrdemServicoUseCase useCase, [FromBody] CriarOrdemServicoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            var errorDescription = result.Error.Description;
            var isNotFound = errorDescription.Contains("encontrado", StringComparison.OrdinalIgnoreCase)
                && (errorDescription.Contains("Cliente", StringComparison.OrdinalIgnoreCase)
                    || errorDescription.Contains("Veiculo", StringComparison.OrdinalIgnoreCase));

            if (isNotFound)
                return NotFound(new { error = errorDescription });

            return BadRequest(new { error = errorDescription });
        }

        return CreatedAtAction(nameof(Post), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPost("{id:guid}/addservico")]
    [ProducesResponseType(typeof(AdicionarServicoOrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddServico(
        [FromRoute] Guid id,
        [FromServices] IAdicionarServicoOrdemServicoUseCase useCase,
        [FromBody] AdicionarServicoOrdemServicoCommand command,
        CancellationToken cancellationToken)
    {
        var addCommand = new AdicionarServicoOrdemServicoCommand
        {
            OrdemServicoId = id,
            ServicoId = command.ServicoId,
            Quantidade = command.Quantidade
        };

        var result = await useCase.ExecuteAsync(addCommand, cancellationToken);

        if (!result.IsSuccess)
        {
            var errorDescription = result.Error.Description;
            var isNotFound = errorDescription.Contains("encontrad", StringComparison.OrdinalIgnoreCase)
                && (errorDescription.Contains("Ordem", StringComparison.OrdinalIgnoreCase)
                    || errorDescription.Contains("Servico", StringComparison.OrdinalIgnoreCase));

            if (isNotFound)
                return NotFound(new { error = errorDescription });

            return BadRequest(new { error = errorDescription });
        }

        return CreatedAtAction(nameof(AddServico), new { id = result.Value!.OrdemServicoId, servicoDaOrdemServicoId = result.Value.Id }, result.Value);
    }
}
