using Elo.Application.DTOs.Assinatura;
using Elo.Application.UseCases.Assinaturas;
using Elo.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Elo.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AssinaturasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public AssinaturasController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AssinaturaDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateAssinatura.Command command)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? userId = null;
        if(int.TryParse(userIdString, out var uid)) userId = uid;

        command.EmpresaId = empresaId;
        command.UsuarioId = userId;

        var result = await _mediator.Send(command);
        return Created("", result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AssinaturaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var query = new GetAllAssinaturas.Query { EmpresaId = empresaId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(int id)
    {
        var empresaId = await _empresaContext.RequireEmpresaAsync(null, HttpContext.RequestAborted);
        var command = new CancelarAssinatura.Command 
        { 
            AssinaturaId = id, 
            EmpresaId = empresaId 
        };
        await _mediator.Send(command);
        return NoContent();
    }
}
