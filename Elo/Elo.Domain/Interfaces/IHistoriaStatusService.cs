using Elo.Domain.Entities;

namespace Elo.Domain.Interfaces;

public interface IHistoriaStatusService
{
    Task<HistoriaStatus> CriarAsync(string nome, string? cor, int ordem, bool fechaHistoria, bool ativo, int? empresaId);
    Task<HistoriaStatus> AtualizarAsync(int id, string nome, string? cor, int ordem, bool fechaHistoria, bool ativo, int? empresaId);
    Task<bool> DeletarAsync(int id);
    Task<HistoriaStatus?> ObterPorIdAsync(int id);
    Task<IEnumerable<HistoriaStatus>> ObterTodosAsync(int? empresaId = null);
    Task<IEnumerable<HistoriaStatus>> ObterPorListaIdsAsync(IEnumerable<int> ids);
}
