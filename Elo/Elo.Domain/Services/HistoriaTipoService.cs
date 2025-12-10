using Elo.Domain.Entities;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class HistoriaTipoService : IHistoriaTipoService
{
    private readonly IUnitOfWork _unitOfWork;

    public HistoriaTipoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<HistoriaTipo> CriarAsync(string nome, string? descricao, int ordem, bool ativo, int? empresaId)
    {
        var tipo = new HistoriaTipo
        {
            Nome = nome,
            Descricao = descricao,
            Ordem = ordem,
            Ativo = ativo,
            EmpresaId = empresaId
        };

        var created = await _unitOfWork.HistoriaTipos.AddAsync(tipo);
        await _unitOfWork.SaveChangesAsync();
        return created;
    }

    public async Task<HistoriaTipo> AtualizarAsync(int id, string nome, string? descricao, int ordem, bool ativo, int? empresaId)
    {
        var tipo = await _unitOfWork.HistoriaTipos.GetByIdAsync(id);
        if (tipo == null)
            throw new KeyNotFoundException($"HistoriaTipo com ID {id} não encontrado");

        tipo.Nome = nome;
        tipo.Descricao = descricao;
        tipo.Ordem = ordem;
        tipo.Ativo = ativo;
        tipo.EmpresaId = empresaId;

        await _unitOfWork.HistoriaTipos.UpdateAsync(tipo);
        await _unitOfWork.SaveChangesAsync();
        return tipo;
    }

    public async Task<bool> DeletarAsync(int id)
    {
        var tipo = await _unitOfWork.HistoriaTipos.GetByIdAsync(id);
        if (tipo == null)
            throw new KeyNotFoundException($"HistoriaTipo com ID {id} não encontrado");

        await _unitOfWork.HistoriaTipos.DeleteAsync(tipo);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<HistoriaTipo?> ObterPorIdAsync(int id)
    {
        return await _unitOfWork.HistoriaTipos.GetByIdAsync(id);
    }

    public async Task<IEnumerable<HistoriaTipo>> ObterTodosAsync(int? empresaId = null)
    {
        var tipos = await _unitOfWork.HistoriaTipos.GetAllAsync();
        if (empresaId.HasValue)
            tipos = tipos.Where(t => !t.EmpresaId.HasValue || t.EmpresaId == empresaId.Value);
        return tipos;
    }

    public async Task<IEnumerable<HistoriaTipo>> ObterPorListaIdsAsync(IEnumerable<int> ids)
    {
        if (!ids.Any()) return Enumerable.Empty<HistoriaTipo>();
        var idList = ids.Distinct().ToList();
        return await _unitOfWork.HistoriaTipos.FindAsync(t => idList.Contains(t.Id));
    }
}
