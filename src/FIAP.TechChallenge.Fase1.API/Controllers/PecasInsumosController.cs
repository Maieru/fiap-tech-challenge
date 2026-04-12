using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.EntradaEstoquePecaInsumo;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.AtualizarPecaInsumo;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.IncluirPecaInsumo;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PecasInsumosController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(IncluirPecaInsumoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromServices] IIncluirPecaInsumoUseCase useCase, [FromBody] IncluirPecaInsumoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error.Description });

        return CreatedAtAction(nameof(Post), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}/entrada-estoque")]
    [ProducesResponseType(typeof(EntradaEstoquePecaInsumoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutEntradaEstoque(
        [FromRoute] Guid id,
        [FromServices] IEntradaEstoquePecaInsumoUseCase useCase,
        [FromBody] EntradaEstoquePecaInsumoCommand command,
        CancellationToken cancellationToken)
    {
        var entradaEstoqueCommand = new EntradaEstoquePecaInsumoCommand
        {
            Id = id,
            Quantidade = command.Quantidade
        };

        var result = await useCase.ExecuteAsync(entradaEstoqueCommand, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error.Description.Contains("Peca ou insumo", StringComparison.OrdinalIgnoreCase)
                && result.Error.Description.Contains("encontrado", StringComparison.OrdinalIgnoreCase))

                return NotFound(new { error = result.Error.Description });

            return BadRequest(new { error = result.Error.Description });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AtualizarPecaInsumoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromServices] IAtualizarPecaInsumoUseCase useCase, [FromBody] AtualizarPecaInsumoCommand command, CancellationToken cancellationToken)
    {
        var updateCommand = new AtualizarPecaInsumoCommand
        {
            Id = id,
            Nome = command.Nome,
            Codigo = command.Codigo,
            Descricao = command.Descricao,
            PrecoUnitario = command.PrecoUnitario,
            Ativo = command.Ativo
        };

        var result = await useCase.ExecuteAsync(updateCommand, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error.Description.Contains("Peca ou insumo", StringComparison.OrdinalIgnoreCase)
                && result.Error.Description.Contains("encontrado", StringComparison.OrdinalIgnoreCase))

                return NotFound(new { error = result.Error.Description });

            return BadRequest(new { error = result.Error.Description });
        }

        return Ok(result.Value);
    }
}
