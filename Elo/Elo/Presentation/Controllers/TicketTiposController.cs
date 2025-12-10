using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Ticket;
using Elo.Application.UseCases.TicketTipos;
using Elo.Domain.Interfaces;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/ticket-tipos")]
[Authorize]
public class TicketTiposController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public TicketTiposController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketTipoDto>>> GetAll([FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAllTicketTipos.Query { EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TicketTipoDto>> Create([FromBody] CreateTicketTipoDto dto, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.RequireEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var command = new CreateTicketTipo.Command
        {
            EmpresaId = resolvedEmpresa,
            Dto = dto
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TicketTipoDto>> Update(int id, [FromBody] UpdateTicketTipoDto dto, [FromQuery] int? empresaId)
    {
        if (dto.Id != 0 && dto.Id != id)
        {
            return BadRequest(new { message = "ID inválido" });
        }

        dto.Id = id;
        var resolvedEmpresa = await _empresaContext.RequireEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var command = new UpdateTicketTipo.Command
        {
            EmpresaId = resolvedEmpresa,
            Dto = dto
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.RequireEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var command = new DeleteTicketTipo.Command { Id = id, EmpresaId = resolvedEmpresa };
        await _mediator.Send(command);
        return NoContent();
    }
}
