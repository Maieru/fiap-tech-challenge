using FIAP.TechChallenge.Fase1.API.Extensions;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AprovarExecucaoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConcluirServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.EntregarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.FinalizarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ListarOrdensServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.RecuperarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.SolicitarAprovacaoOrdemServico;
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

    [HttpPut("{id:guid}/solicitar-aprovacao")]
    [ProducesResponseType(typeof(SolicitarAprovacaoOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutSolicitarAprovacao(
        [FromRoute] Guid id,
        [FromServices] ISolicitarAprovacaoOrdemServicoUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new SolicitarAprovacaoOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/aprovar-execucao")]
    [ProducesResponseType(typeof(AprovarExecucaoOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutAprovarExecucao(
        [FromRoute] Guid id,
        [FromServices] IAprovarExecucaoOrdemServicoUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new AprovarExecucaoOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("servicos/{servicoDaOrdemServicoId:guid}/concluir")]
    [ProducesResponseType(typeof(ConcluirServicoOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutConcluirServico(
        [FromRoute] Guid servicoDaOrdemServicoId,
        [FromServices] IConcluirServicoOrdemServicoUseCase useCase,
        [FromBody] ConcluirServicoOrdemServicoCommand command,
        CancellationToken cancellationToken)
    {
        var concluirCommand = new ConcluirServicoOrdemServicoCommand
        {
            ServicoDaOrdemDeServicoId = servicoDaOrdemServicoId,
            TempoGastoMinutos = command.TempoGastoMinutos
        };

        var result = await useCase.ExecuteAsync(concluirCommand, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/finalizar")]
    [ProducesResponseType(typeof(FinalizarOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutFinalizar(
        [FromRoute] Guid id,
        [FromServices] IFinalizarOrdemServicoUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new FinalizarOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/entregar")]
    [ProducesResponseType(typeof(EntregarOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutEntregar(
        [FromRoute] Guid id,
        [FromServices] IEntregarOrdemServicoUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new EntregarOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }
}
