using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Cliente;
using Elo.Application.UseCases.Clientes;
using Elo.Application.Interfaces;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public ClientesController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAllClientes.Query
        {
            Page = page,
            PageSize = pageSize,
            EmpresaId = resolvedEmpresa
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetClienteById.Query { Id = id, EmpresaId = resolvedEmpresa };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create([FromBody] CreateClienteDto dto)
    {
        try
        {
            var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
            var command = new CreateCliente.Command
            {
                Nome = dto.Nome,
                CnpjCpf = dto.CnpjCpf,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Status = dto.Status.ToString(),
                Enderecos = dto.Enderecos,
                EmpresaId = empresaId
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClienteDto>> Update(int id, [FromBody] UpdateClienteDto dto)
    {
        try
        {
            var empresaIdResolved = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
            var command = new UpdateCliente.Command
            {
                Id = id,
                Nome = dto.Nome,
                CnpjCpf = dto.CnpjCpf,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Status = dto.Status.ToString(),
                Enderecos = dto.Enderecos,
                EmpresaId = empresaIdResolved
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
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
            var command = new DeleteCliente.Command { Id = id, EmpresaId = empresaId };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
