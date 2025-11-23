using Elo.Domain.Entities;

namespace Elo.Domain.Interfaces;

public interface IUserService
{
    Task<bool> EmailJaExisteAsync(string email, int? userIdExcluido = null);
    Task<User> CriarUsuarioAsync(string nome, string email, string password, string role, int? empresaId = null);
    Task<User?> ValidarCredenciaisAsync(string email, string password);
    Task<User?> ObterUsuarioPorIdAsync(int id);
    Task<User?> ObterUsuarioPorEmailAsync(string email);
    Task<User> AtualizarUsuarioAsync(int id, string nome, string email, string role, int? empresaId = null);
    Task<bool> DeletarUsuarioAsync(int id);
    Task<bool> AlterarSenhaAsync(int id, string currentPassword, string newPassword);
    Task<IEnumerable<User>> ObterTodosUsuariosAsync(int? empresaId = null);
}
