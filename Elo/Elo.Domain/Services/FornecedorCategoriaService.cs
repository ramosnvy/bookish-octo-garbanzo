using Elo.Domain.Entities;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class FornecedorCategoriaService : IFornecedorCategoriaService
{
    private readonly IUnitOfWork _unitOfWork;

    public FornecedorCategoriaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FornecedorCategoria> CriarAsync(string nome, bool ativo, int empresaId)
    {
        await ValidarDuplicidadeAsync(nome, empresaId);

        var categoria = new FornecedorCategoria
        {
            EmpresaId = empresaId,
            Nome = nome,
            Ativo = ativo
        };

        await _unitOfWork.FornecedorCategorias.AddAsync(categoria);
        await _unitOfWork.SaveChangesAsync();
        return categoria;
    }

    public async Task<FornecedorCategoria> AtualizarAsync(int id, string nome, bool ativo, int empresaId)
    {
        var categoria = await _unitOfWork.FornecedorCategorias.GetByIdAsync(id);
        if (categoria == null || categoria.EmpresaId != empresaId)
        {
            throw new FornecedorCategoriaNaoEncontradaException(id);
        }

        await ValidarDuplicidadeAsync(nome, empresaId, id);

        categoria.Nome = nome;
        categoria.Ativo = ativo;
        categoria.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.FornecedorCategorias.UpdateAsync(categoria);
        await _unitOfWork.SaveChangesAsync();
        return categoria;
    }

    public async Task<bool> DeletarAsync(int id, int empresaId)
    {
        var categoria = await _unitOfWork.FornecedorCategorias.GetByIdAsync(id);
        if (categoria == null || categoria.EmpresaId != empresaId)
        {
            throw new FornecedorCategoriaNaoEncontradaException(id);
        }

        await _unitOfWork.FornecedorCategorias.DeleteAsync(categoria);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<FornecedorCategoria?> ObterPorIdAsync(int id, int empresaId)
    {
        var categoria = await _unitOfWork.FornecedorCategorias.GetByIdAsync(id);
        return categoria != null && categoria.EmpresaId == empresaId ? categoria : null;
    }

    public async Task<IEnumerable<FornecedorCategoria>> ObterTodasAsync(int? empresaId = null)
    {
        if (empresaId.HasValue)
        {
            return await _unitOfWork.FornecedorCategorias.FindAsync(c => c.EmpresaId == empresaId);
        }

        return await _unitOfWork.FornecedorCategorias.GetAllAsync();
    }

    private async Task ValidarDuplicidadeAsync(string nome, int empresaId, int? id = null)
    {
        var existente = await _unitOfWork.FornecedorCategorias.FirstOrDefaultAsync(c => c.Nome == nome && c.EmpresaId == empresaId);
        if (existente != null && existente.Id != id)
        {
            throw new ClienteJaExisteException("Já existe uma categoria com esse nome.");
        }
    }
}
