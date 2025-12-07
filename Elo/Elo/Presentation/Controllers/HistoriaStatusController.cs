using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Historia;
using Elo.Application.Interfaces;
using Elo.Application.UseCases.HistoriaStatuses;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/historia-status")]
[Authorize]
public class HistoriaStatusController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public HistoriaStatusController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HistoriaStatusDto>>> GetAll([FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAllHistoriaStatus.Query { EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<HistoriaStatusDto>> Create([FromBody] CreateHistoriaStatusDto dto, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.RequireEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var command = new CreateHistoriaStatus.Command
        {
            EmpresaId = resolvedEmpresa,
            Dto = dto
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<HistoriaStatusDto>> Update(int id, [FromBody] UpdateHistoriaStatusDto dto, [FromQuery] int? empresaId)
    {
        if (dto.Id != 0 && dto.Id != id)
        {
            return BadRequest(new { message = "ID inválido" });
        }

        dto.Id = id;
        var resolvedEmpresa = await _empresaContext.RequireEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var command = new UpdateHistoriaStatus.Command
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
        var command = new DeleteHistoriaStatus.Command { Id = id, EmpresaId = resolvedEmpresa };
        await _mediator.Send(command);
        return NoContent();
    }
}
