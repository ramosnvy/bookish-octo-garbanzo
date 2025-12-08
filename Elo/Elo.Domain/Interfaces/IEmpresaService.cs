using Elo.Domain.Entities;

namespace Elo.Domain.Interfaces;

public interface IEmpresaService
{
    Task<Empresa> CriarEmpresaAsync(Empresa empresa);
    Task<IEnumerable<Empresa>> ObterTodasAsync();
    Task<Empresa?> ObterPorIdAsync(int id);
    Task<Empresa> AtualizarEmpresaAsync(Empresa empresa);
}
