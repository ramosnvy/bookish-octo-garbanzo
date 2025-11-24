using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Historia;
using Elo.Application.UseCases.Historias;
using Elo.Application.Interfaces;
using System.Security.Claims;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoriasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public HistoriasController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HistoriaDto>>> GetAll(
        [FromQuery] Domain.Enums.HistoriaStatus? status,
        [FromQuery] Domain.Enums.HistoriaTipo? tipo,
        [FromQuery] int? clienteId,
        [FromQuery] int? produtoId,
        [FromQuery] int? usuarioResponsavelId,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAllHistorias.Query
        {
            EmpresaId = resolvedEmpresa,
            Status = status,
            Tipo = tipo,
            ClienteId = clienteId,
            ProdutoId = produtoId,
            UsuarioResponsavelId = usuarioResponsavelId,
            DataInicio = dataInicio,
            DataFim = dataFim
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HistoriaDto>> GetById(int id, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetHistoriaById.Query { Id = id, EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<HistoriaDto>> Create([FromBody] CreateHistoriaDto dto)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new CreateHistoria.Command
        {
            EmpresaId = empresaId,
            Dto = dto,
            RequesterUserId = userId.Value,
            IsGlobalAdmin = _empresaContext.IsGlobalAdmin
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<HistoriaDto>> Update(int id, [FromBody] UpdateHistoriaDto dto)
    {
        if (dto.Id != 0 && dto.Id != id)
        {
            return BadRequest(new { message = "ID inválido" });
        }

        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        dto.Id = id;

        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new UpdateHistoria.Command
        {
            EmpresaId = empresaId,
            Dto = dto,
            RequesterUserId = userId.Value,
            IsGlobalAdmin = _empresaContext.IsGlobalAdmin
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new DeleteHistoria.Command { Id = id, EmpresaId = empresaId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/movimentacoes")]
    public async Task<ActionResult<HistoriaDto>> AddMovimentacao(int id, [FromBody] CreateHistoriaMovimentacaoDto dto)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new AddHistoriaMovimentacao.Command
        {
            HistoriaId = id,
            EmpresaId = empresaId,
            UsuarioId = userId.Value,
            Dto = dto
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out var id) ? id : null;
    }
}
