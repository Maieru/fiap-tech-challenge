using FIAP.TechChallenge.Fase1.API.Extensions;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.AtualizarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ExcluirCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ListarClientes;
using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.RecuperarCliente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ClientesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListarClientesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(IListarClientesUseCase useCase, string? cpf, string? cnpj, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var command = new ListarClientesCommand
        {
            Cpf = cpf,
            Cnpj = cnpj,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await useCase.ExecuteAsync(command, cancellationToken);

        return result.ToActionResult(this, value =>
        {
            if (!string.IsNullOrWhiteSpace(cpf) || !string.IsNullOrWhiteSpace(cnpj))
                return Ok(value.Clientes.First());

            return Ok(value);
        });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecuperarClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, IRecuperarClienteUseCase useCase, CancellationToken cancellationToken = default)
    {
        var command = new RecuperarClienteCommand { ClienteId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CriarClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(ICriarClienteUseCase useCase, CriarClienteCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(Post), new { id = value.Id }, value));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AtualizarClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, IAtualizarClienteUseCase useCase, AtualizarClienteCommand command, CancellationToken cancellationToken)
    {
        var updateCommand = new AtualizarClienteCommand
        {
            Id = id,
            Nome = command.Nome,
            Telefone = command.Telefone,
            Email = command.Email
        };

        var result = await useCase.ExecuteAsync(updateCommand, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, IExcluirClienteUseCase useCase, CancellationToken cancellationToken)
    {
        var command = new ExcluirClienteCommand { Id = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, _ => NoContent());
    }
}
