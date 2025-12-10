using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Fornecedor;
using Elo.Application.UseCases.Fornecedores;
using Elo.Domain.Interfaces;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FornecedoresController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public FornecedoresController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? categoria, [FromQuery] string? status, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAllFornecedores.Query
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            Categoria = categoria,
            Status = status,
            EmpresaId = resolvedEmpresa
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FornecedorDto>> GetById(int id, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetFornecedorById.Query { Id = id, EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<FornecedorDto>> Create([FromBody] CreateFornecedorDto dto)
    {
        try
        {
            var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
            var command = new CreateFornecedor.Command
            {
                Nome = dto.Nome,
                Cnpj = dto.Cnpj,
                Email = dto.Email,
                Telefone = dto.Telefone,
                CategoriaId = dto.CategoriaId,
                Status = dto.Status.ToString(),
                TipoPagamentoServico = dto.TipoPagamentoServico,
                PrazoPagamentoDias = dto.PrazoPagamentoDias,
                Enderecos = dto.Enderecos,
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
    public async Task<ActionResult<FornecedorDto>> Update(int id, [FromBody] UpdateFornecedorDto dto)
    {
        try
        {
            var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
            var command = new UpdateFornecedor.Command
            {
                Id = id,
                Nome = dto.Nome,
                Cnpj = dto.Cnpj,
                Email = dto.Email,
                Telefone = dto.Telefone,
                CategoriaId = dto.CategoriaId,
                Status = dto.Status.ToString(),
                TipoPagamentoServico = dto.TipoPagamentoServico,
                PrazoPagamentoDias = dto.PrazoPagamentoDias,
                Enderecos = dto.Enderecos,
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

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
            var command = new DeleteFornecedor.Command { Id = id, EmpresaId = empresaId };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
