using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Produto;
using Elo.Application.UseCases.Produtos;
using Elo.Domain.Interfaces;
using System.Linq;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public ProdutosController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] bool? ativo, [FromQuery] decimal? valorMinimo, [FromQuery] decimal? valorMaximo, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAllProdutos.Query
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            Ativo = ativo,
            ValorMinimo = valorMinimo,
            ValorMaximo = valorMaximo,
            EmpresaId = resolvedEmpresa
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProdutoDto>> GetById(int id, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetProdutoById.Query { Id = id, EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> Create([FromBody] CreateProdutoDto dto)
    {
        try
        {
            var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
            var command = new CreateProduto.Command
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                ValorCusto = dto.ValorCusto,
                ValorRevenda = dto.ValorRevenda,
                Ativo = dto.Ativo,
                FornecedorId = dto.FornecedorId,
                Modulos = dto.Modulos ?? Enumerable.Empty<ProdutoModuloInputDto>(),
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
    public async Task<ActionResult<ProdutoDto>> Update(int id, [FromBody] UpdateProdutoDto dto)
    {
        try
        {
            var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
            var command = new UpdateProduto.Command
            {
                Id = id,
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                ValorCusto = dto.ValorCusto,
                ValorRevenda = dto.ValorRevenda,
                Ativo = dto.Ativo,
                FornecedorId = dto.FornecedorId,
                Modulos = dto.Modulos ?? Enumerable.Empty<ProdutoModuloInputDto>(),
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
            var command = new DeleteProduto.Command { Id = id, EmpresaId = empresaId };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("calcular-margem")]
    public async Task<ActionResult<decimal>> CalcularMargem([FromBody] CalcularMargemDto dto)
    {
        try
        {
            var command = new CalcularMargem.Command
            {
                ValorCusto = dto.ValorCusto,
                ValorRevenda = dto.ValorRevenda
            };

            var result = await _mediator.Send(command);
            return Ok(new { margemLucro = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
