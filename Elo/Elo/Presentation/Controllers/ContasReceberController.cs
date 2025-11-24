using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Financeiro;
using Elo.Application.UseCases.ContasReceber;
using Elo.Application.Interfaces;
using Elo.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/financeiro/contas-receber")]
[Authorize]
public class ContasReceberController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public ContasReceberController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContaReceberDto>>> GetAll([FromQuery] ContaStatus? status, [FromQuery] DateTime? dataInicial, [FromQuery] DateTime? dataFinal, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAllContasReceber.Query
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
    public async Task<ActionResult<ContaReceberDto>> GetById(int id, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetContaReceberById.Query { Id = id, EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ContaReceberDto>> Create([FromBody] CreateContaReceberDto dto)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new CreateContaReceber.Command { EmpresaId = empresaId, Dto = dto };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ContaReceberDto>> Update(int id, [FromBody] UpdateContaReceberDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { message = "ID inválido" });
        }

        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new UpdateContaReceber.Command { EmpresaId = empresaId, Dto = dto };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new DeleteContaReceber.Command { Id = id, EmpresaId = empresaId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut("{contaId}/parcelas/{parcelaId}/status")]
    public async Task<ActionResult<ContaReceberParcelaDto>> UpdateParcelaStatus(int contaId, int parcelaId, [FromBody] UpdateContaReceberParcelaStatusDto dto)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new UpdateContaReceberParcelaStatus.Command
        {
            EmpresaId = empresaId,
            ContaId = contaId,
            ParcelaId = parcelaId,
            Status = dto.Status,
            DataRecebimento = dto.DataRecebimento
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
