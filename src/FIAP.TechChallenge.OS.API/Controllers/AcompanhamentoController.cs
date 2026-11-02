using FIAP.TechChallenge.OS.API.Extensions;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.EmitirAcessoCliente;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarAcompanhamento;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.AutorizarExecucao;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.SolicitarCancelamento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FIAP.TechChallenge.OS.API.Controllers;
[ApiController, Route("api/os/v1/ordens")]
public sealed class AcompanhamentoController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        base.OnActionExecuting(context);
    }
    [Authorize, HttpPost("{id:guid}/acesso-cliente")]
    [ProducesResponseType(typeof(EmitirAcessoClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Emitir(Guid id, [FromServices] IEmitirAcessoClienteUseCase useCase, CancellationToken ct) =>
        (await useCase.ExecuteAsync(new(id), ct)).ToActionResult(this);
    [AllowAnonymous, HttpGet("acompanhamento/{id:guid}")]
    [ProducesResponseType(typeof(AcompanhamentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Consultar(Guid id, [FromQuery] string? token, [FromServices] IConsultarAcompanhamentoUseCase useCase, CancellationToken ct) =>
        (await useCase.ExecuteAsync(new(id, token ?? string.Empty), ct)).ToActionResult(this);
    [AllowAnonymous, HttpPut("acompanhamento/{id:guid}/aprovar-execucao")]
    [ProducesResponseType(typeof(DecisaoClienteResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Aprovar(Guid id, [FromQuery] string? token, [FromServices] IAutorizarExecucaoUseCase useCase, CancellationToken ct) =>
        (await useCase.ExecuteAsync(new(id, token ?? string.Empty), ct)).ToActionResult(this, value => Accepted(DecisaoClienteResponse.From(value)));
    [AllowAnonymous, HttpPut("acompanhamento/{id:guid}/cancelar")]
    [ProducesResponseType(typeof(DecisaoClienteResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancelar(Guid id, [FromQuery] string? token, [FromServices] ISolicitarCancelamentoUseCase useCase, CancellationToken ct) =>
        (await useCase.ExecuteAsync(new(id, token ?? string.Empty), ct)).ToActionResult(this, value => Accepted(DecisaoClienteResponse.From(value)));
}
