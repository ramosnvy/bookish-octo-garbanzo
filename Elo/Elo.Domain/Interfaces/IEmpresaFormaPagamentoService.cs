using Elo.Domain.Entities;
using Elo.Domain.Enums;

namespace Elo.Domain.Interfaces;

public interface IEmpresaFormaPagamentoService
{
    Task<EmpresaFormaPagamento> CriarFormaPagamentoAsync(int empresaId, FormaPagamento formaPagamento, string nome, bool aVista);
    Task<IEnumerable<EmpresaFormaPagamento>> ObterFormasPagamentoPorEmpresaAsync(int empresaId, bool apenasAtivos = true);
    Task<EmpresaFormaPagamento?> ObterFormaPagamentoPorIdAsync(int id, int empresaId);
    Task AtualizarStatusFormaPagamentoAsync(int id, int empresaId, bool ativo);
    Task DeletarFormaPagamentoAsync(int id, int empresaId);
}
