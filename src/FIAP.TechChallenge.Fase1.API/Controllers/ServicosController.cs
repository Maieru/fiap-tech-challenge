using FIAP.TechChallenge.Fase1.API.Extensions;
using FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.AtualizarServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.CadastrarServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.ListarServicos;
using FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.RecuperarServico;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ServicosController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListarServicosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(IListarServicosUseCase useCase, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var command = new ListarServicosCommand
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecuperarServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, IRecuperarServicoUseCase useCase, CancellationToken cancellationToken = default)
    {
        var command = new RecuperarServicoCommand { ServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CadastrarServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post(ICadastrarServicoUseCase useCase, CadastrarServicoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(Post), new { id = value.Id }, value));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AtualizarServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, IAtualizarServicoUseCase useCase, AtualizarServicoCommand command, CancellationToken cancellationToken)
    {
        var updateCommand = new AtualizarServicoCommand
        {
            Id = id,
            Descricao = command.Descricao,
            ValorUnitario = command.ValorUnitario
        };

        var result = await useCase.ExecuteAsync(updateCommand, cancellationToken);
        return result.ToActionResult(this);
    }
}
