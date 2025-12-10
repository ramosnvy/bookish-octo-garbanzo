using Elo.Domain.Entities;
using Elo.Domain.Models;

namespace Elo.Domain.Interfaces;

public interface IProdutoService
{
    Task<Produto> CriarProdutoAsync(string nome, string descricao, decimal valorCusto, decimal valorRevenda, bool ativo, int? fornecedorId, IEnumerable<ProdutoModuloInput> modulos, int empresaId);
    Task<Produto> AtualizarProdutoAsync(int id, string nome, string descricao, decimal valorCusto, decimal valorRevenda, bool ativo, int? fornecedorId, IEnumerable<ProdutoModuloInput> modulos, int empresaId);
    Task<bool> DeletarProdutoAsync(int id, int empresaId);
    Task<Produto?> ObterProdutoPorIdAsync(int id, int? empresaId = null);
    Task<IEnumerable<Produto>> ObterTodosProdutosAsync(int? empresaId = null);
    Task<IEnumerable<Produto>> ObterProdutosPorIdsAsync(IEnumerable<int> ids);
    Task<IEnumerable<ProdutoModulo>> ObterModulosPorIdsAsync(IEnumerable<int> ids);
    decimal CalcularMargemLucro(decimal valorCusto, decimal valorRevenda);
}
