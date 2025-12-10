using Elo.Domain.Entities;
using Elo.Domain.Enums;

namespace Elo.Domain.Interfaces;

public interface IAfiliadoService
{
    Task<Afiliado> CriarAfiliadoAsync(string nome, string email, string documento, string telefone, decimal porcentagem, Status status, int empresaId);
    Task<Afiliado> AtualizarAfiliadoAsync(int id, string nome, string email, string documento, string telefone, decimal porcentagem, Status status, int empresaId);
    Task<Afiliado> AtualizarStatusAfiliadoAsync(int id, Status status, int empresaId);
    Task<bool> DeletarAfiliadoAsync(int id, int empresaId);
    Task<Afiliado?> ObterAfiliadoPorIdAsync(int id, int empresaId);
    Task<IEnumerable<Afiliado>> ObterAfiliadosAsync(int? empresaId = null);
}
