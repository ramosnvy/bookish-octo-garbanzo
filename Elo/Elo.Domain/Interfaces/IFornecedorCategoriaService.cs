using Elo.Domain.Entities;

namespace Elo.Domain.Interfaces;

public interface IFornecedorCategoriaService
{
    Task<FornecedorCategoria> CriarAsync(string nome, bool ativo, int empresaId);
    Task<FornecedorCategoria> AtualizarAsync(int id, string nome, bool ativo, int empresaId);
    Task<bool> DeletarAsync(int id, int empresaId);
    Task<FornecedorCategoria?> ObterPorIdAsync(int id, int empresaId);
    Task<IEnumerable<FornecedorCategoria>> ObterTodasAsync(int? empresaId = null);
}
