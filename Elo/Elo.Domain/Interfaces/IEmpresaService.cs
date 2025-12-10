using Elo.Domain.Entities;

namespace Elo.Domain.Interfaces;

public interface IEmpresaService
{
    Task<Empresa> CriarEmpresaAsync(string razaoSocial, string nomeFantasia, string cnpj, string ie, string email, string telefone, string endereco, bool ativo);
    Task<Empresa> AtualizarEmpresaAsync(int id, string razaoSocial, string nomeFantasia, string cnpj, string ie, string email, string telefone, string endereco, bool ativo);
    Task<bool> DeletarEmpresaAsync(int id);
    Task<IEnumerable<Empresa>> ObterTodasAsync();
    Task<Empresa?> ObterPorIdAsync(int id);
    Task<Empresa> AtualizarStatusAsync(int id, bool ativo);
}
