using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Ticket;
using Elo.Application.UseCases.Tickets;
using Elo.Application.Interfaces;
using System.Security.Claims;
using System.IO;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public TicketsController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll(
        [FromQuery] Domain.Enums.TicketStatus? status,
        [FromQuery] int? tipoId,
        [FromQuery] Domain.Enums.TicketPrioridade? prioridade,
        [FromQuery] int? clienteId,
        [FromQuery] int? produtoId,
        [FromQuery] int? fornecedorId,
        [FromQuery] int? usuarioAtribuidoId,
        [FromQuery] DateTime? dataAberturaInicio,
        [FromQuery] DateTime? dataAberturaFim,
        [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
            var query = new GetAllTickets.Query
            {
                EmpresaId = resolvedEmpresa,
                Status = status,
                TipoId = tipoId,
                Prioridade = prioridade,
                ClienteId = clienteId,
                ProdutoId = produtoId,
                FornecedorId = fornecedorId,
                UsuarioAtribuidoId = usuarioAtribuidoId,
                DataAberturaInicio = dataAberturaInicio,
                DataAberturaFim = dataAberturaFim
            };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TicketDto>> GetById(int id, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetTicketById.Query { Id = id, EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketDto dto)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new CreateTicket.Command { EmpresaId = empresaId, Dto = dto };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TicketDto>> Update(int id, [FromBody] UpdateTicketDto dto)
    {
        if (dto.Id != 0 && dto.Id != id)
        {
            return BadRequest(new { message = "ID inválido" });
        }

        dto.Id = id;
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new UpdateTicket.Command { EmpresaId = empresaId, Dto = dto };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new DeleteTicket.Command { Id = id, EmpresaId = empresaId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/respostas")]
    public async Task<ActionResult<TicketDto>> Responder(int id, [FromBody] CreateRespostaTicketDto dto)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new CreateRespostaTicket.Command
        {
            TicketId = id,
            EmpresaId = empresaId,
            UsuarioId = userId.Value,
            Dto = dto
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/anexos")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<TicketDto>> Anexar(int id, IFormFile file)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Selecione um arquivo válido." });
        }

        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        await using var memory = new MemoryStream();
        await file.CopyToAsync(memory);

        var command = new CreateTicketAnexo.Command
        {
            TicketId = id,
            EmpresaId = empresaId,
            UsuarioId = userId.Value,
            NomeArquivo = string.IsNullOrWhiteSpace(file.FileName) ? "anexo" : file.FileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            Conteudo = memory.ToArray()
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
