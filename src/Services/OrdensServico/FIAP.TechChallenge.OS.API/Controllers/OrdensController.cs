using FIAP.TechChallenge.OS.API.Extensions;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.AutorizarExecucao;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarHistorico;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarOrdem;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.CriarOrdemServico;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ListarOrdensServico;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.SolicitarCancelamento;
using FIAP.TechChallenge.OS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.OS.API.Controllers;

[ApiController]
[Route("api/os/v1/ordens")]
[Authorize]
public sealed class OrdensController : ControllerBase
{
    [HttpPut("{id:guid}/aprovar-execucao")]
    [ProducesResponseType(typeof(DecisaoExecucaoResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(Guid id, IAutorizarExecucaoUseCase useCase, CancellationToken cancellationToken) =>
        (await useCase.ExecuteAsync(new(id), cancellationToken)).ToActionResult(this, value => AcceptedAtAction(nameof(GetById), new { id }, value));

    [HttpPut("{id:guid}/cancelar")]
    [ProducesResponseType(typeof(DecisaoExecucaoResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, ISolicitarCancelamentoUseCase useCase, CancellationToken cancellationToken) =>
        (await useCase.ExecuteAsync(new(id), cancellationToken)).ToActionResult(this, value => AcceptedAtAction(nameof(GetById), new { id }, value));

    [HttpPost]
    [ProducesResponseType(typeof(CriarOrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post(CriarOrdemServicoCommand command, ICriarOrdemServicoUseCase useCase, CancellationToken cancellationToken) =>
        (await useCase.ExecuteAsync(command, cancellationToken)).ToActionResult(this, value => CreatedAtAction(nameof(GetById), new { id = value.Id }, value));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrdemServico), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, IConsultarOrdemUseCase useCase, CancellationToken cancellationToken) =>
        (await useCase.ExecuteAsync(id, cancellationToken)).ToActionResult(this);

    [HttpGet]
    [ProducesResponseType(typeof(ListarOrdensServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromQuery] ListarOrdensServicoCommand command, IListarOrdensServicoUseCase useCase, CancellationToken cancellationToken) =>
        (await useCase.ExecuteAsync(command, cancellationToken)).ToActionResult(this);

    [HttpGet("{id:guid}/historico")]
    [ProducesResponseType(typeof(IReadOnlyCollection<HistoricoOrdem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(Guid id, IConsultarHistoricoUseCase useCase, CancellationToken cancellationToken) =>
        (await useCase.ExecuteAsync(id, cancellationToken)).ToActionResult(this);
}
