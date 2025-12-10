using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Financeiro;
using Elo.Application.UseCases.ContasPagar;
using Elo.Domain.Interfaces;
using Elo.Domain.Enums;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/financeiro/contas-pagar")]
[Authorize]
public class ContasPagarController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public ContasPagarController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContaPagarDto>>> GetAll([FromQuery] ContaStatus? status, [FromQuery] DateTime? dataInicial, [FromQuery] DateTime? dataFinal, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAllContasPagar.Query
        {
            EmpresaId = resolvedEmpresa,
            Status = status,
            DataInicial = dataInicial,
            DataFinal = dataFinal
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("calendario")]
    public async Task<ActionResult<IEnumerable<ContaPagarCalendarEventDto>>> GetCalendar([FromQuery] ContaStatus? status, [FromQuery] DateTime? dataInicial, [FromQuery] DateTime? dataFinal, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetContasPagarCalendar.Query
        {
            EmpresaId = resolvedEmpresa,
            Status = status,
            DataInicial = dataInicial,
            DataFinal = dataFinal
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContaPagarDto>> GetById(int id, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetContaPagarById.Query { Id = id, EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ContaPagarDto>> Create([FromBody] CreateContaPagarDto dto)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new CreateContaPagar.Command { EmpresaId = empresaId, Dto = dto };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ContaPagarDto>> Update(int id, [FromBody] UpdateContaPagarDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { message = "ID inválido" });
        }

        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new UpdateContaPagar.Command { EmpresaId = empresaId, Dto = dto };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ContaPagarDto>> UpdateStatus(int id, [FromBody] UpdateContaPagarStatusDto dto)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new UpdateContaPagarStatus.Command 
        { 
            Id = id,
            EmpresaId = empresaId, 
            Status = dto.Status,
            DataPagamento = dto.DataPagamento
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new DeleteContaPagar.Command { Id = id, EmpresaId = empresaId };
        await _mediator.Send(command);
        return NoContent();
    }
}
