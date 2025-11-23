using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmpresaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Empresa> CriarEmpresaAsync(Empresa empresa, string usuarioNome, string usuarioEmail, string usuarioPassword)
    {
        if (await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == usuarioEmail) != null)
        {
            throw new ClienteJaExisteException("Já existe um usuário com este email");
        }

        await _unitOfWork.Empresas.AddAsync(empresa);
        await _unitOfWork.SaveChangesAsync();

        var user = new User
        {
            Nome = usuarioNome,
            Email = usuarioEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuarioPassword),
            Role = UserRole.Admin,
            EmpresaId = empresa.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return empresa;
    }

    public async Task<IEnumerable<Empresa>> ObterTodasAsync()
    {
        return await _unitOfWork.Empresas.GetAllAsync();
    }

    public async Task<Empresa?> ObterPorIdAsync(int id)
    {
        return await _unitOfWork.Empresas.GetByIdAsync(id);
    }

    public async Task<Empresa> AtualizarEmpresaAsync(Empresa empresa)
    {
        await _unitOfWork.Empresas.UpdateAsync(empresa);
        await _unitOfWork.SaveChangesAsync();
        return empresa;
    }
}
