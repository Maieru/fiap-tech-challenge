using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.AtualizarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ListarClientes;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ClientesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListarClientesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromServices] IListarClientesUseCase useCase,
        [FromQuery] Guid? id,
        [FromQuery] string? cpf,
        [FromQuery] string? cnpj,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var command = new ListarClientesCommand
        {
            Id = id,
            Cpf = cpf,
            Cnpj = cnpj,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await useCase.ExecuteAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error.Description == "Cliente não encontrado.")
                return NotFound(new { error = result.Error.Description });

            return BadRequest(new { error = result.Error.Description });
        }

        if (id.HasValue || !string.IsNullOrWhiteSpace(cpf) || !string.IsNullOrWhiteSpace(cnpj))
            return Ok(result.Value!.Clientes.First());

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CriarClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromServices] ICriarClienteUseCase useCase, [FromBody] CriarClienteCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error.Description });

        return CreatedAtAction(nameof(Post), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AtualizarClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromServices] IAtualizarClienteUseCase useCase, [FromBody] AtualizarClienteCommand command, CancellationToken cancellationToken)
    {
        var updateCommand = new AtualizarClienteCommand
        {
            Id = id,
            Nome = command.Nome,
            Telefone = command.Telefone,
            Email = command.Email
        };

        var result = await useCase.ExecuteAsync(updateCommand, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error.Description == "Cliente não encontrado.")
                return NotFound(new { error = result.Error.Description });

            return BadRequest(new { error = result.Error.Description });
        }

        return Ok(result.Value);
    }
}