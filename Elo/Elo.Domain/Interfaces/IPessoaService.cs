using Elo.Domain.Entities;
using Elo.Domain.Enums;

namespace Elo.Domain.Interfaces;

public interface IPessoaService
{
    Task<Pessoa> CriarPessoaAsync(PessoaTipo tipo, string nome, string documento, string email, string telefone, Status status, int? categoriaId, IEnumerable<PessoaEndereco> enderecos, int empresaId, ServicoPagamentoTipo? servicoPagamentoTipo = null, int? prazoPagamentoDias = null);
    Task<Pessoa> AtualizarPessoaAsync(int id, PessoaTipo tipo, string nome, string documento, string email, string telefone, Status status, int? categoriaId, IEnumerable<PessoaEndereco> enderecos, int empresaId, ServicoPagamentoTipo? servicoPagamentoTipo = null, int? prazoPagamentoDias = null);
    Task<bool> DeletarPessoaAsync(int id, PessoaTipo tipo, int empresaId);
    Task<Pessoa?> ObterPessoaPorIdAsync(int id, PessoaTipo tipo, int? empresaId = null);
    Task<IEnumerable<Pessoa>> ObterPessoasAsync(PessoaTipo tipo, int? empresaId = null);
    Task<IEnumerable<Pessoa>> ObterPessoasPorIdsAsync(IEnumerable<int> ids, int? empresaId = null);
}
