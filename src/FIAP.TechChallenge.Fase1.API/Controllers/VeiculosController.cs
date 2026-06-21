using FIAP.TechChallenge.Fase1.API.Extensions;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.AtualizarVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.CriarVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ExcluirVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ListarVeiculos;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.RecuperarVeiculo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class VeiculosController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListarVeiculosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(IListarVeiculosUseCase useCase, [FromQuery] ListarVeiculosCommand command, CancellationToken cancellationToken = default)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);

        return result.ToActionResult(this, value =>
        {
            if (!string.IsNullOrWhiteSpace(command.Placa))
                return Ok(value.Veiculos.First());

            return Ok(value);
        });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecuperarVeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, IRecuperarVeiculoUseCase useCase, CancellationToken cancellationToken = default)
    {
        var command = new RecuperarVeiculoCommand { VeiculoId = id };

        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CriarVeiculoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post(ICriarVeiculoUseCase useCase, CriarVeiculoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(Post), new { id = value.Id }, value));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AtualizarVeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, IAtualizarVeiculoUseCase useCase, AtualizarVeiculoCommand command, CancellationToken cancellationToken)
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

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, IExcluirVeiculoUseCase useCase, CancellationToken cancellationToken)
    {
        var command = new ExcluirVeiculoCommand { Id = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, _ => NoContent());
    }
}
