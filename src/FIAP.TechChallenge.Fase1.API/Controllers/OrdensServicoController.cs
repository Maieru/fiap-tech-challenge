using FIAP.TechChallenge.Fase1.API.Extensions;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AcompanhamentoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarPecaInsumoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AdicionarServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.AprovarExecucaoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CancelarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConcluirServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConsultarStatusOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoComClienteEVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoCompleta;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.EntregarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ExcluirOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.FinalizarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.IniciarDiagnosticoOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ListarOrdensServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.RecuperarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.SolicitarAprovacaoOrdemServico;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.TechChallenge.Fase1.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OrdensServicoController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListarOrdensServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(IListarOrdensServicoUseCase useCase, [FromQuery] ListarOrdensServicoCommand command, CancellationToken cancellationToken = default)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecuperarOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(Guid id, IRecuperarOrdemServicoUseCase useCase, CancellationToken cancellationToken = default)
    {
        var command = new RecuperarOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [AllowAnonymous]
    [HttpGet("acompanhamento/{id:guid}")]
    [ProducesResponseType(typeof(AcompanhamentoOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAcompanhamentoById(Guid id, IAcompanhamentoOrdemServicoUseCase useCase, CancellationToken cancellationToken = default)
    {
        var command = new AcompanhamentoOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(ConsultarStatusOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStatusById(Guid id, IConsultarStatusOrdemServicoUseCase useCase, CancellationToken cancellationToken = default)
    {
        var command = new ConsultarStatusOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CriarOrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post(ICriarOrdemServicoUseCase useCase, CriarOrdemServicoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(Post), new { id = value.Id }, value));
    }

    [HttpPost("com-cliente-veiculo")]
    [ProducesResponseType(typeof(CriarOrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostComClienteEVeiculo(ICriarOrdemServicoComClienteEVeiculoUseCase useCase, CriarOrdemServicoComClienteEVeiculoCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(GetById), new { id = value.Id }, value));
    }

    [HttpPost("completa")]
    [ProducesResponseType(typeof(CriarOrdemServicoCompletaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostCompleta(ICriarOrdemServicoCompletaUseCase useCase, CriarOrdemServicoCompletaCommand command, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, value => CreatedAtAction(nameof(GetById), new { id = value.Id }, value));
    }

    [HttpPost("{id:guid}/addservico")]
    [ProducesResponseType(typeof(AdicionarServicoOrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddServico(Guid id, IAdicionarServicoOrdemServicoUseCase useCase, AdicionarServicoOrdemServicoCommand command, CancellationToken cancellationToken)
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
    public async Task<IActionResult> AddPecaInsumo(Guid id, IAdicionarPecaInsumoOrdemServicoUseCase useCase, AdicionarPecaInsumoOrdemServicoCommand command, CancellationToken cancellationToken)
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
    public async Task<IActionResult> PutIniciarDiagnostico(Guid id, IIniciarDiagnosticoOrdemServicoUseCase useCase, CancellationToken cancellationToken)
    {
        var command = new IniciarDiagnosticoOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/solicitar-aprovacao")]
    [ProducesResponseType(typeof(SolicitarAprovacaoOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutSolicitarAprovacao(Guid id, ISolicitarAprovacaoOrdemServicoUseCase useCase, CancellationToken cancellationToken)
    {
        var command = new SolicitarAprovacaoOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/aprovar-execucao")]
    [ProducesResponseType(typeof(AprovarExecucaoOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> PutAprovarExecucao(Guid id, IAprovarExecucaoOrdemServicoUseCase useCase, AprovarExecucaoOrdemServicoCommand command, CancellationToken cancellationToken)
    {
        var aprovarCommand = new AprovarExecucaoOrdemServicoCommand
        {
            OrdemServicoId = id,
            CodigoAprovacao = command.CodigoAprovacao
        };

        var result = await useCase.ExecuteAsync(aprovarCommand, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/cancelar")]
    [ProducesResponseType(typeof(CancelarOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> PutCancelar(Guid id, ICancelarOrdemServicoUseCase useCase, CancellationToken cancellationToken)
    {
        var command = new CancelarOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("servicos/{servicoDaOrdemServicoId:guid}/concluir")]
    [ProducesResponseType(typeof(ConcluirServicoOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutConcluirServico(Guid servicoDaOrdemServicoId, IConcluirServicoOrdemServicoUseCase useCase, ConcluirServicoOrdemServicoCommand command, CancellationToken cancellationToken)
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
    public async Task<IActionResult> PutFinalizar(Guid id, IFinalizarOrdemServicoUseCase useCase, CancellationToken cancellationToken)
    {
        var command = new FinalizarOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}/entregar")]
    [ProducesResponseType(typeof(EntregarOrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutEntregar(Guid id, IEntregarOrdemServicoUseCase useCase, CancellationToken cancellationToken)
    {
        var command = new EntregarOrdemServicoCommand { OrdemServicoId = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, IExcluirOrdemServicoUseCase useCase, CancellationToken cancellationToken)
    {
        var command = new ExcluirOrdemServicoCommand { Id = id };
        var result = await useCase.ExecuteAsync(command, cancellationToken);
        return result.ToActionResult(this, _ => NoContent());
    }
}

