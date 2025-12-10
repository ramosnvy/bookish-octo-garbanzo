using Elo.Domain.Entities;

namespace Elo.Domain.Interfaces;

public interface ITicketTipoService
{
    Task<TicketTipo> CriarAsync(string nome, string? descricao, int ordem, bool ativo, int? empresaId);
    Task<TicketTipo> AtualizarAsync(int id, string nome, string? descricao, int ordem, bool ativo, int? empresaId);
    Task<bool> DeletarAsync(int id);
    Task<TicketTipo?> ObterPorIdAsync(int id);
    Task<IEnumerable<TicketTipo>> ObterTodosAsync(int? empresaId = null);
}
