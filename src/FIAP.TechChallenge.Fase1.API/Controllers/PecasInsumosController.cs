using FIAP.TechChallenge.Fase1.API.Extensions;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.AtualizarPecaInsumo;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.EntradaEstoquePecaInsumo;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.IncluirPecaInsumo;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.ListarPecasInsumos;
using FIAP.TechChallenge.Fase1.Application.UseCases.PecasInsumos.RecuperarPecaInsumo;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PecasInsumosController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListarPecasInsumosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromServices] IListarPecasInsumosUseCase useCase,
        [FromQuery] string? codigo,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var command = new ListarPecasInsumosCommand
        {
            Codigo = codigo,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await useCase.ExecuteAsync(command, cancellationToken);

        return result.ToActionResult(this, value =>
        {
            if (!string.IsNullOrWhiteSpace(codigo))
                return Ok(value.PecasInsumos.First());

            return Ok(value);
        });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecuperarPecaInsumoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, [FromServices] IRecuperarPecaInsumoUseCase useCase, CancellationToken cancellationToken = default)
    {
        var command = new RecuperarPecaInsumoCommand
        {
            PecaInsumoId = id
        };

        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(IncluirPecaInsumoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromServices] IIncluirPecaInsumoUseCase useCase, [FromBody] IncluirPecaInsumoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);

        return result.ToActionResult(this, value => CreatedAtAction(nameof(Post), new { id = value.Id }, value));
    }

    [HttpPut("{id:guid}/entrada-estoque")]
    [ProducesResponseType(typeof(EntradaEstoquePecaInsumoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutEntradaEstoque([FromRoute] Guid id, [FromServices] IEntradaEstoquePecaInsumoUseCase useCase, [FromBody] EntradaEstoquePecaInsumoCommand command, CancellationToken cancellationToken)
    {
        var entradaEstoqueCommand = new EntradaEstoquePecaInsumoCommand
        {
            Id = id,
            Quantidade = command.Quantidade
        };

        var result = await useCase.ExecuteAsync(entradaEstoqueCommand, cancellationToken);
        return result.ToActionResult(this);
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
        return result.ToActionResult(this);
    }
}
