using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.AtualizarVeiculo;
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AtualizarVeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromServices] IAtualizarVeiculoUseCase useCase, [FromBody] AtualizarVeiculoCommand command, CancellationToken cancellationToken)
    {
        var updateCommand = new AtualizarVeiculoCommand
        {
            Id = id,
            Placa = command.Placa,
            Marca = command.Marca,
            Modelo = command.Modelo,
            Ano = command.Ano
        };

        var result = await useCase.ExecuteAsync(updateCommand, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error.Description.Contains("Veículo", StringComparison.OrdinalIgnoreCase)
                && result.Error.Description.Contains("encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { error = result.Error.Description });

            return BadRequest(new { error = result.Error.Description });
        }

        return Ok(result.Value);
    }
}
