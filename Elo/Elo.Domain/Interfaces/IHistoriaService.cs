using Elo.Domain.Entities;
using Elo.Domain.Enums;

namespace Elo.Domain.Interfaces;

public interface IHistoriaService
{
    Task<Historia> CriarHistoriaAsync(int clienteId, int produtoId, int historiaStatusId, int historiaTipoId, int? usuarioResponsavelId, string? observacoes, int? previsaoDias, int empresaId, IEnumerable<HistoriaProdutoInput>? produtos = null, int? usuarioCriadorId = null);
    
    Task<Historia> AtualizarHistoriaAsync(int id, int clienteId, int produtoId, int historiaStatusId, int historiaTipoId, int? usuarioResponsavelId, string? observacoes, int? previsaoDias, int empresaId, IEnumerable<HistoriaProdutoInput>? produtos = null, int? usuarioAlteracaoId = null);
    
    Task<bool> DeletarHistoriaAsync(int id, int empresaId);
    Task<Historia?> ObterHistoriaPorIdAsync(int id, int empresaId);
    Task<IEnumerable<Historia>> ObterHistoriasAsync(int? empresaId = null, int? clienteId = null, int? statusId = null);
    
    Task<HistoriaMovimentacao> AdicionarMovimentacaoAsync(int historiaId, int statusAnteriorId, int statusNovoId, int usuarioId, string? observacoes, int empresaId);

    Task<IEnumerable<HistoriaProduto>> ObterProdutosPorHistoriaIdAsync(int historiaId);
    Task<IEnumerable<HistoriaProduto>> ObterProdutosPorListaIdsAsync(IEnumerable<int> historiaIds);
    Task<IEnumerable<HistoriaMovimentacao>> ObterMovimentacoesPorHistoriaIdAsync(int historiaId);
    Task<IEnumerable<HistoriaMovimentacao>> ObterMovimentacoesPorListaIdsAsync(IEnumerable<int> historiaIds);
}

public record HistoriaProdutoInput(
    int ProdutoId,
    List<int>? ProdutoModuloIds);
