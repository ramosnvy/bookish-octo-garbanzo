using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;
using System;

namespace Elo.Domain.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? userIdExcluido = null)
    {
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user != null && user.Id != userIdExcluido;
    }

    public async Task<User> CriarUsuarioAsync(string nome, string email, string password, string role, int? empresaId = null, Status status = Status.Ativo)
    {
        // Validações de negócio
        if (await EmailJaExisteAsync(email))
        {
            throw new ClienteJaExisteException("Já existe um usuário com este email");
        }

        if (empresaId.HasValue)
        {
            var empresaExiste = await _unitOfWork.Empresas.GetByIdAsync(empresaId.Value);
            if (empresaExiste == null)
            {
                throw new InvalidOperationException("Empresa informada não existe.");
            }
            
            if (!empresaExiste.Ativo)
            {
                throw new EmpresaInativaException("Não é possível criar usuário para empresa inativa.");
            }
        }

        // Criação da entidade
        var user = new User
        {
            Nome = nome,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password),
            Role = Enum.Parse<UserRole>(role),
            Status = status,
            EmpresaId = empresaId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task<User?> ValidarCredenciaisAsync(string email, string password)
    {
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        // Validar status do usuário
        if (user.Status != Status.Ativo)
        {
            throw new UsuarioInativoException("Usuário inativo. Entre em contato com o administrador.");
        }

        // Validar se a empresa está ativa (se o usuário pertence a uma empresa)
        if (user.EmpresaId.HasValue)
        {
            var empresa = await _unitOfWork.Empresas.GetByIdAsync(user.EmpresaId.Value);
            if (empresa == null || !empresa.Ativo)
            {
                throw new EmpresaInativaException("Empresa inativa. Entre em contato com o administrador.");
            }
        }

        return user;
    }

    public async Task<User?> ObterUsuarioPorIdAsync(int id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }

    public async Task<User?> ObterUsuarioPorEmailAsync(string email)
    {
        return await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> AtualizarUsuarioAsync(int id, string nome, string email, string role, int? empresaId = null, Status? status = null)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new ClienteNaoEncontradoException(id);
        }

        // Validações de negócio
        if (await EmailJaExisteAsync(email, id))
        {
            throw new ClienteJaExisteException("Já existe outro usuário com este email");
        }

        if (empresaId.HasValue)
        {
            var empresaExiste = await _unitOfWork.Empresas.GetByIdAsync(empresaId.Value);
            if (empresaExiste == null)
            {
                throw new InvalidOperationException("Empresa informada não existe.");
            }
            
            if (!empresaExiste.Ativo)
            {
                throw new EmpresaInativaException("Não é possível associar usuário a empresa inativa.");
            }
        }

        // Atualização da entidade
        user.Nome = nome;
        user.Email = email;
        user.Role = Enum.Parse<UserRole>(role);
        if (status.HasValue)
        {
            user.Status = status.Value;
        }
        user.EmpresaId = empresaId;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task<bool> DeletarUsuarioAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new ClienteNaoEncontradoException(id);
        }

        await _unitOfWork.Users.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AlterarSenhaAsync(int id, string currentPassword, string newPassword)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            throw new ClienteNaoEncontradoException(id);
        }

        if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
        {
            throw new ClienteJaExisteException("Senha atual incorreta");
        }

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<User>> ObterTodosUsuariosAsync(int? empresaId = null)
    {
        if (empresaId.HasValue)
        {
            return await _unitOfWork.Users.FindAsync(u => u.EmpresaId == empresaId);
        }

        return await _unitOfWork.Users.GetAllAsync();
    }

    public async Task<IEnumerable<User>> ObterUsuariosPorIdsAsync(IEnumerable<int> ids)
    {
        if (!ids.Any()) return Enumerable.Empty<User>();
        var idList = ids.Distinct().ToList();
        return await _unitOfWork.Users.FindAsync(u => idList.Contains(u.Id));
    }
}
