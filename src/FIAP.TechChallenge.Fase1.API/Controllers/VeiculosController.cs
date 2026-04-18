using FIAP.TechChallenge.Fase1.API.Extensions;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.AtualizarVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.CriarVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ListarVeiculos;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.RecuperarVeiculo;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VeiculosController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListarVeiculosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromServices] IListarVeiculosUseCase useCase,
        [FromQuery] string? placa,
        [FromQuery] Guid? clienteId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var command = new ListarVeiculosCommand
        {
            Placa = placa,
            ClienteId = clienteId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await useCase.ExecuteAsync(command, cancellationToken);

        return result.ToActionResult(this, value =>
        {
            if (!string.IsNullOrWhiteSpace(placa))
                return Ok(value.Veiculos.First());

            return Ok(value);
        });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecuperarVeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, [FromServices] IRecuperarVeiculoUseCase useCase, CancellationToken cancellationToken = default)
    {
        var command = new RecuperarVeiculoCommand { VeiculoId = id };

        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CriarVeiculoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post([FromServices] ICriarVeiculoUseCase useCase, [FromBody] CriarVeiculoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(Post), new { id = value.Id }, value));
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
        return result.ToActionResult(this);
    }
}
