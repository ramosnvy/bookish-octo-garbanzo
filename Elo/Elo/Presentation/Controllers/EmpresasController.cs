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
            RazaoSocial = dto.RazaoSocial,
            NomeFantasia = dto.NomeFantasia,
            Cnpj = dto.Cnpj,
            Ie = dto.Ie,
            Email = dto.Email,
            Telefone = dto.Telefone,
            Endereco = dto.Endereco,
            Ativo = dto.Ativo
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
            RazaoSocial = dto.RazaoSocial,
            NomeFantasia = dto.NomeFantasia,
            Cnpj = dto.Cnpj,
            Ie = dto.Ie,
            Email = dto.Email,
            Telefone = dto.Telefone,
            Endereco = dto.Endereco,
            Ativo = dto.Ativo
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
