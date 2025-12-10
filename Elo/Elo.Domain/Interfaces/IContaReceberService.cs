using Elo.Domain.Entities;
using Elo.Domain.Enums;

namespace Elo.Domain.Interfaces;

public interface IContaReceberService
{
    Task<ContaReceber> CriarContaReceberAsync(
        int clienteId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        DateTime? dataRecebimento,
        ContaStatus status,
        FormaPagamento formaPagamento,
        int? numeroParcelas,
        int? intervaloDias,
        int empresaId,
        IEnumerable<ContaReceberItemInput>? itens = null,
        bool isRecorrente = false,
        int? assinaturaId = null);

    Task<ContaReceber> AtualizarContaReceberAsync(
        int id,
        int clienteId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        DateTime? dataRecebimento,
        ContaStatus status,
        FormaPagamento formaPagamento,
        int empresaId);

    Task<IEnumerable<ContaReceberItem>> ObterItensPorContaIdAsync(int contaId);
    Task<IEnumerable<ContaReceberItem>> ObterItensPorListaIdsAsync(IEnumerable<int> contaIds);
    Task<IEnumerable<ContaReceberParcela>> ObterParcelasPorContaIdAsync(int contaId);
    Task<IEnumerable<ContaReceberParcela>> ObterParcelasPorListaIdsAsync(IEnumerable<int> contaIds);

    Task<bool> DeletarContaReceberAsync(int id, int empresaId);
    
    Task<ContaReceber?> ObterContaReceberPorIdAsync(int id, int empresaId);
    
    Task<IEnumerable<ContaReceber>> ObterContasReceberAsync(
        int? empresaId = null,
        int? clienteId = null,
        ContaStatus? status = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null);

    Task<ContaReceberParcela> AtualizarStatusParcelaAsync(
        int parcelaId,
        ContaStatus novoStatus,
        DateTime? dataRecebimento,
        int empresaId);
}

public record ContaReceberItemInput(
    string Descricao,
    decimal Valor);
