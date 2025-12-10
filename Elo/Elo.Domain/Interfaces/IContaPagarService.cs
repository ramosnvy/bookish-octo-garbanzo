using Elo.Domain.Entities;
using Elo.Domain.Enums;

namespace Elo.Domain.Interfaces;

public interface IContaPagarService
{
    Task<ContaPagar> CriarContaPagarAsync(
        int? fornecedorId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        DateTime? dataPagamento,
        ContaStatus status,
        string categoria,
        bool isRecorrente,
        int? numeroParcelas,
        int? intervaloDias,
        int empresaId,
        IEnumerable<ContaPagarItemInput>? itens = null,
        int? assinaturaId = null,
        int? afiliadoId = null);

    Task<ContaPagar> AtualizarContaPagarAsync(
        int id,
        int? fornecedorId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        DateTime? dataPagamento,
        ContaStatus status,
        string categoria,
        bool isRecorrente,
        int empresaId,
        int? afiliadoId = null);

    Task<bool> DeletarContaPagarAsync(int id, int empresaId);
    
    Task<ContaPagar?> ObterContaPagarPorIdAsync(int id, int empresaId);
    
    Task<IEnumerable<ContaPagar>> ObterContasPagarAsync(
        int? empresaId = null,
        int? fornecedorId = null,
        ContaStatus? status = null,
        string? categoria = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null);

    Task<IEnumerable<ContaPagarItem>> ObterItensPorContaIdAsync(int contaId);
    Task<IEnumerable<ContaPagarItem>> ObterItensPorListaIdsAsync(IEnumerable<int> contaIds);
    Task<IEnumerable<ContaPagarParcela>> ObterParcelasPorContaIdAsync(int contaId);
    Task<IEnumerable<ContaPagarParcela>> ObterParcelasPorListaIdsAsync(IEnumerable<int> contaIds);

    Task<ContaPagarParcela> AtualizarStatusParcelaAsync(
        int parcelaId,
        ContaStatus novoStatus,
        DateTime? dataPagamento,
        int empresaId);
}

public record ContaPagarItemInput(
    string Descricao,
    decimal Valor,
    int? ProdutoId,
    List<int>? ProdutoModuloIds);
