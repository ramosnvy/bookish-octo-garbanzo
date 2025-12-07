using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Historia;
using Elo.Application.Interfaces;
using Elo.Application.UseCases.HistoriaTipos;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/historia-tipos")]
[Authorize]
public class HistoriaTiposController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public HistoriaTiposController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HistoriaTipoDto>>> GetAll([FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAllHistoriaTipos.Query { EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<HistoriaTipoDto>> Create([FromBody] CreateHistoriaTipoDto dto, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.RequireEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var command = new CreateHistoriaTipo.Command
        {
            EmpresaId = resolvedEmpresa,
            Dto = dto
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<HistoriaTipoDto>> Update(int id, [FromBody] UpdateHistoriaTipoDto dto, [FromQuery] int? empresaId)
    {
        if (dto.Id != 0 && dto.Id != id)
        {
            return BadRequest(new { message = "ID inválido" });
        }

        dto.Id = id;
        var resolvedEmpresa = await _empresaContext.RequireEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var command = new UpdateHistoriaTipo.Command
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
        var command = new DeleteHistoriaTipo.Command { Id = id, EmpresaId = resolvedEmpresa };
        await _mediator.Send(command);
        return NoContent();
    }
}
