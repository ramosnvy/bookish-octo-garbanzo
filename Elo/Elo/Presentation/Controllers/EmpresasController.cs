using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.Empresa;
using Elo.Application.UseCases.Empresas;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class EmpresasController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmpresasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmpresaDto>>> GetAll()
    {
        var query = new GetAllEmpresas.Query();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<EmpresaDto>> Create([FromBody] CreateEmpresaDto dto)
    {
        var command = new CreateEmpresa.Command
        {
            Nome = dto.Nome,
            Documento = dto.Documento,
            EmailContato = dto.EmailContato,
            TelefoneContato = dto.TelefoneContato,
            Ativo = dto.Ativo,
            UsuarioNome = dto.UsuarioInicial.Nome,
            UsuarioEmail = dto.UsuarioInicial.Email,
            UsuarioPassword = dto.UsuarioInicial.Password
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EmpresaDto>> Update(int id, [FromBody] UpdateEmpresaDto dto)
    {
        var command = new UpdateEmpresa.Command
        {
            Id = id,
            Nome = dto.Nome,
            Documento = dto.Documento,
            EmailContato = dto.EmailContato,
            TelefoneContato = dto.TelefoneContato,
            Ativo = dto.Ativo
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
