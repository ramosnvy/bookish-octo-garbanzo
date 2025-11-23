using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Fornecedor;
using Elo.Application.UseCases.FornecedorCategorias;
using Elo.Application.Interfaces;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FornecedorCategoriasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public FornecedorCategoriasController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FornecedorCategoriaDto>>> GetAll([FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var categorias = await _mediator.Send(new GetAllFornecedorCategorias.Query { EmpresaId = resolvedEmpresa });
        return Ok(categorias);
    }

    [HttpPost]
    public async Task<ActionResult<FornecedorCategoriaDto>> Create([FromBody] CreateFornecedorCategoriaDto dto)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new CreateFornecedorCategoria.Command
        {
            Nome = dto.Nome,
            Ativo = dto.Ativo,
            EmpresaId = empresaId
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FornecedorCategoriaDto>> Update(int id, [FromBody] UpdateFornecedorCategoriaDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { message = "ID inválido" });
        }

        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new UpdateFornecedorCategoria.Command
        {
            Id = dto.Id,
            Nome = dto.Nome,
            Ativo = dto.Ativo,
            EmpresaId = empresaId
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new DeleteFornecedorCategoria.Command { Id = id, EmpresaId = empresaId };
        await _mediator.Send(command);
        return NoContent();
    }
}
