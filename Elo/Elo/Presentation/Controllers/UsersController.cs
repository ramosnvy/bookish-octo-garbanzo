using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elo.Application.DTOs.User;
using Elo.Application.UseCases.Users;
using Elo.Domain.Interfaces;

namespace Elo.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmpresaContextService _empresaContext;

    public UsersController(IMediator mediator, IEmpresaContextService empresaContext)
    {
        _mediator = mediator;
        _empresaContext = empresaContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? search, [FromQuery] string? role, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetAllUsers.Query
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            Role = role,
            EmpresaId = resolvedEmpresa
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id, [FromQuery] int? empresaId)
    {
        var resolvedEmpresa = await _empresaContext.ResolveEmpresaAsync(empresaId, HttpContext.RequestAborted);
        var query = new GetUserById.Query { Id = id, EmpresaId = resolvedEmpresa, IsGlobalAdmin = _empresaContext.IsGlobalAdmin };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var isGlobalAdmin = _empresaContext.IsGlobalAdmin;
            var command = new CreateUser.Command
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Password = dto.Password,
                Role = dto.Role.ToString(),
                EmpresaId = isGlobalAdmin ? dto.EmpresaId : _empresaContext.UsuarioEmpresaId,
                IsGlobalAdmin = isGlobalAdmin,
                RequesterEmpresaId = _empresaContext.UsuarioEmpresaId
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
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var isGlobalAdmin = _empresaContext.IsGlobalAdmin;
            var command = new UpdateUser.Command
            {
                Id = id,
                Nome = dto.Nome,
                Email = dto.Email,
                Role = dto.Role.ToString(),
                EmpresaId = isGlobalAdmin ? dto.EmpresaId : _empresaContext.UsuarioEmpresaId,
                IsGlobalAdmin = isGlobalAdmin,
                RequesterEmpresaId = _empresaContext.UsuarioEmpresaId
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
            var command = new DeleteUser.Command { Id = id, IsGlobalAdmin = _empresaContext.IsGlobalAdmin, RequesterEmpresaId = _empresaContext.UsuarioEmpresaId };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/change-password")]
    public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
    {
        try
        {
            var command = new ChangePassword.Command
            {
                Id = id,
                CurrentPassword = dto.CurrentPassword,
                NewPassword = dto.NewPassword
            };

            await _mediator.Send(command);
            return Ok(new { message = "Senha alterada com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
