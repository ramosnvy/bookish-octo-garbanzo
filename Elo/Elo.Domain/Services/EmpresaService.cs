using Elo.Domain.Entities;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmpresaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Empresa> CriarEmpresaAsync(Empresa empresa)
    {
        await _unitOfWork.Empresas.AddAsync(empresa);
        await _unitOfWork.SaveChangesAsync();

        return empresa;
    }

    public async Task<IEnumerable<Empresa>> ObterTodasAsync()
    {
        return await _unitOfWork.Empresas.GetAllAsync();
    }

    public async Task<Empresa?> ObterPorIdAsync(int id)
    {
        return await _unitOfWork.Empresas.GetByIdAsync(id);
    }

    public async Task<Empresa> AtualizarEmpresaAsync(Empresa empresa)
    {
        await _unitOfWork.Empresas.UpdateAsync(empresa);
        await _unitOfWork.SaveChangesAsync();
        return empresa;
    }
}
