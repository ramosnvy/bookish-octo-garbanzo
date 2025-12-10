using Elo.Domain.Entities;

namespace Elo.Domain.Interfaces;

public interface IHistoriaTipoService
{
    Task<HistoriaTipo> CriarAsync(string nome, string? descricao, int ordem, bool ativo, int? empresaId);
    Task<HistoriaTipo> AtualizarAsync(int id, string nome, string? descricao, int ordem, bool ativo, int? empresaId);
    Task<bool> DeletarAsync(int id);
    Task<HistoriaTipo?> ObterPorIdAsync(int id);
    Task<IEnumerable<HistoriaTipo>> ObterTodosAsync(int? empresaId = null);
    Task<IEnumerable<HistoriaTipo>> ObterPorListaIdsAsync(IEnumerable<int> ids);
}
