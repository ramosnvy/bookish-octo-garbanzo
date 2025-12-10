using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Afiliado;
using Elo.Application.UseCases.Afiliados;
using Elo.Domain.Interfaces;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AfiliadosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public AfiliadosController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AfiliadoDto>>> GetAll([FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAllAfiliados.Query { EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AfiliadoDto>> GetById(int id, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAfiliadoById.Query { Id = id, EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);

        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AfiliadoDto>> Create([FromBody] CreateAfiliadoDto dto)
    {
        try
        {
            var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
            var command = new CreateAfiliado.Command
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Documento = dto.Documento,
                Telefone = dto.Telefone,
                Porcentagem = dto.Porcentagem,
                Status = dto.Status,
                EmpresaId = empresaId
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AfiliadoDto>> Update(int id, [FromBody] UpdateAfiliadoDto dto)
    {
        try
        {
            var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
            var command = new UpdateAfiliado.Command
            {
                Id = id,
                Nome = dto.Nome,
                Email = dto.Email,
                Documento = dto.Documento,
                Telefone = dto.Telefone,
                Porcentagem = dto.Porcentagem,
                Status = dto.Status,
                EmpresaId = empresaId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<AfiliadoDto>> UpdateStatus(int id, [FromBody] UpdateAfiliadoStatus.Command command)
    {
        try
        {
            var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
            command.Id = id;
            command.EmpresaId = empresaId;

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
            var command = new DeleteAfiliado.Command { Id = id, EmpresaId = empresaId };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
