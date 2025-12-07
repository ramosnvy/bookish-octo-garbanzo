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

    public async Task<User> CriarUsuarioAsync(string nome, string email, string password, string role, int? empresaId = null)
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
        }

        // Criação da entidade
        var user = new User
        {
            Nome = nome,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password),
            Role = Enum.Parse<UserRole>(role),
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

    public async Task<User> AtualizarUsuarioAsync(int id, string nome, string email, string role, int? empresaId = null)
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
        }

        // Atualização da entidade
        user.Nome = nome;
        user.Email = email;
        user.Role = Enum.Parse<UserRole>(role);
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
}
