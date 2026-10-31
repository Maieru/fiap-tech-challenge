using FIAP.TechChallenge.OS.API.Extensions;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarHistorico;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarOrdem;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.CriarOrdemServico;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ListarOrdensServico;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.SolicitarFluxo;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.OS.API.Controllers;

[ApiController]
[Route("api/os/v1/ordens")]
[Authorize]
public sealed class OrdensController : ControllerBase
{
    [HttpPost("{id:guid}/autorizar-execucao")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Autorizar(Guid id, ISolicitarFluxoUseCase useCase, IConfiguration configuration,
        IWebHostEnvironment environment, CancellationToken cancellationToken)
    {
        if (!(environment.IsDevelopment() || environment.IsEnvironment("Testing")) || !configuration.GetValue<bool>("ManualExecutionAuthorizationEnabled"))
            return Task.FromResult<IActionResult>(NotFound());
        return Solicitar(id, "Autorizacao", useCase, cancellationToken);
    }

    [HttpPost("{id:guid}/cancelar")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> Cancelar(Guid id, ISolicitarFluxoUseCase useCase, CancellationToken cancellationToken) =>
        Solicitar(id, "Cancelamento", useCase, cancellationToken);

    private async Task<IActionResult> Solicitar(Guid id, string acao, ISolicitarFluxoUseCase useCase, CancellationToken ct) =>
        (await useCase.ExecuteAsync(id, acao, User.FindFirstValue("sub"), ct)).ToActionResult(this,
            value => AcceptedAtAction(nameof(GetById), new { id }, value));

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
