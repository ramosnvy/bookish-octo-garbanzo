using Elo.Domain.Entities;
using Elo.Domain.Exceptions;
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

    public async Task<Empresa> CriarEmpresaAsync(string razaoSocial, string nomeFantasia, string cnpj, string ie, string email, string telefone, string endereco, bool ativo)
    {
        if (!string.IsNullOrWhiteSpace(cnpj))
        {
            var existing = await _unitOfWork.Empresas.FirstOrDefaultAsync(e => e.Cnpj == cnpj);
            if (existing != null)
                throw new ClienteJaExisteException("Já existe uma empresa com este CNPJ.");
        }

        var empresa = new Empresa
        {
            RazaoSocial = razaoSocial,
            NomeFantasia = nomeFantasia,
            Cnpj = cnpj,
            Ie = ie,
            Email = email,
            Telefone = telefone,
            Endereco = endereco,
            Ativo = ativo,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.Empresas.AddAsync(empresa);
        await _unitOfWork.SaveChangesAsync();

        return created;
    }

    public async Task<Empresa> AtualizarEmpresaAsync(int id, string razaoSocial, string nomeFantasia, string cnpj, string ie, string email, string telefone, string endereco, bool ativo)
    {
        var empresa = await _unitOfWork.Empresas.GetByIdAsync(id);
        if (empresa == null)
            throw new KeyNotFoundException("Empresa não encontrada.");

        if (!string.IsNullOrWhiteSpace(cnpj) && cnpj != empresa.Cnpj)
        {
            var existing = await _unitOfWork.Empresas.FirstOrDefaultAsync(e => e.Cnpj == cnpj);
            if (existing != null)
                throw new ClienteJaExisteException("Já existe uma empresa com este CNPJ.");
        }

        empresa.RazaoSocial = razaoSocial;
        empresa.NomeFantasia = nomeFantasia;
        empresa.Cnpj = cnpj;
        empresa.Ie = ie;
        empresa.Email = email;
        empresa.Telefone = telefone;
        empresa.Endereco = endereco;
        empresa.Ativo = ativo;
        empresa.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Empresas.UpdateAsync(empresa);
        await _unitOfWork.SaveChangesAsync();

        return empresa;
    }

    public async Task<bool> DeletarEmpresaAsync(int id)
    {
        var empresa = await _unitOfWork.Empresas.GetByIdAsync(id);
        if (empresa == null)
            throw new KeyNotFoundException("Empresa não encontrada.");

        await _unitOfWork.Empresas.DeleteAsync(empresa);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Empresa>> ObterTodasAsync()
    {
        return await _unitOfWork.Empresas.GetAllAsync();
    }

    public async Task<Empresa?> ObterPorIdAsync(int id)
    {
        return await _unitOfWork.Empresas.GetByIdAsync(id);
    }

    public async Task<Empresa> AtualizarStatusAsync(int id, bool ativo)
    {
        var empresa = await _unitOfWork.Empresas.GetByIdAsync(id);
        if (empresa == null)
            throw new KeyNotFoundException("Empresa não encontrada.");

        empresa.Ativo = ativo;
        empresa.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Empresas.UpdateAsync(empresa);
        await _unitOfWork.SaveChangesAsync();

        return empresa;
    }
}
