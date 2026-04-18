using FIAP.TechChallenge.Fase1.API.Extensions;
using FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.AtualizarServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.Servicos.CadastrarServico;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ServicosController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CadastrarServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post([FromServices] ICadastrarServicoUseCase useCase, [FromBody] CadastrarServicoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(Post), new { id = value.Id }, value));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AtualizarServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromServices] IAtualizarServicoUseCase useCase, [FromBody] AtualizarServicoCommand command, CancellationToken cancellationToken)
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
