using Elo.Application.DTOs.EmpresaFormaPagamento;
using Elo.Application.UseCases.EmpresaFormasPagamento;
using Elo.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpresaFormasPagamentoController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public EmpresaFormasPagamentoController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    /// <summary>
    /// Lista todas as formas de pagamento disponíveis para a empresa.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<EmpresaFormaPagamentoDto>>> GetAll([FromQuery] bool apenasAtivos = true)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var query = new GetAllEmpresaFormasPagamento.Query
        {
            EmpresaId = empresaId,
            ApenasAtivos = apenasAtivos
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Cria uma nova forma de pagamento para a empresa.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EmpresaFormaPagamentoDto>> Create([FromBody] CreateEmpresaFormaPagamento.Command command)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        command.EmpresaId = empresaId;

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    /// <summary>
    /// Remove (soft delete) uma forma de pagamento da empresa.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new DeleteEmpresaFormaPagamento.Command
        {
            Id = id,
            EmpresaId = empresaId
        };

        await _mediator.Send(command);
        return NoContent();
    }
}
