using FIAP.TechChallenge.Fase1.API.Extensions;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ListarOrdensServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.RecuperarOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdensServicoController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListarOrdensServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
        [FromServices] IListarOrdensServicoUseCase useCase,
        [FromQuery] Guid? clienteId,
        [FromQuery] Guid? veiculoId,
        [FromQuery] StatusOrdemServico? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var command = new ListarOrdensServicoCommand
        {
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await useCase.ExecuteAsync(command, cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecuperarOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, [FromServices] IRecuperarOrdemServicoUseCase useCase, CancellationToken cancellationToken = default)
    {
        var command = new RecuperarOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CriarOrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post([FromServices] ICriarOrdemServicoUseCase useCase, [FromBody] CriarOrdemServicoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(Post), new { id = value.Id }, value));
    }

    [HttpPost("{id:guid}/addservico")]
    [ProducesResponseType(typeof(AdicionarServicoOrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddServico([FromRoute] Guid id, [FromServices] IAdicionarServicoOrdemServicoUseCase useCase, [FromBody] AdicionarServicoOrdemServicoCommand command, CancellationToken cancellationToken)
    {
        var addCommand = new AdicionarServicoOrdemServicoCommand
        {
            OrdemServicoId = id,
            ServicoId = command.ServicoId,
            Quantidade = command.Quantidade
        };

        var result = await useCase.ExecuteAsync(addCommand, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(AddServico), new { id = value.OrdemServicoId, servicoDaOrdemServicoId = value.Id }, value));
    }

    [HttpPost("{id:guid}/addpecainsumo")]
    [ProducesResponseType(typeof(AdicionarPecaInsumoOrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPecaInsumo(
        [FromRoute] Guid id,
        [FromServices] IAdicionarPecaInsumoOrdemServicoUseCase useCase,
        [FromBody] AdicionarPecaInsumoOrdemServicoCommand command,
        CancellationToken cancellationToken)
    {
        var addCommand = new AdicionarPecaInsumoOrdemServicoCommand
        {
            OrdemServicoId = id,
            PecaInsumoId = command.PecaInsumoId,
            Quantidade = command.Quantidade
        };

        var result = await useCase.ExecuteAsync(addCommand, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(AddPecaInsumo), new { id = value.OrdemServicoId, pecaOuInsumoDaOrdemServicoId = value.Id }, value));
    }

    [HttpPut("{id:guid}/iniciar-diagnostico")]
    [ProducesResponseType(typeof(IniciarDiagnosticoOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutIniciarDiagnostico(
        [FromRoute] Guid id,
        [FromServices] IIniciarDiagnosticoOrdemServicoUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new IniciarDiagnosticoOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }
}
