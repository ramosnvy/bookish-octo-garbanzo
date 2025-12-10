using Elo.Domain.Entities;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class HistoriaStatusService : IHistoriaStatusService
{
    private readonly IUnitOfWork _unitOfWork;

    public HistoriaStatusService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<HistoriaStatus> CriarAsync(string nome, string? cor, int ordem, bool fechaHistoria, bool ativo, int? empresaId)
    {
        var status = new HistoriaStatus
        {
            Nome = nome,
            Cor = cor,
            Ordem = ordem,
            FechaHistoria = fechaHistoria,
            Ativo = ativo,
            EmpresaId = empresaId
        };

        var created = await _unitOfWork.HistoriaStatuses.AddAsync(status);
        await _unitOfWork.SaveChangesAsync();
        return created;
    }

    public async Task<HistoriaStatus> AtualizarAsync(int id, string nome, string? cor, int ordem, bool fechaHistoria, bool ativo, int? empresaId)
    {
        var status = await _unitOfWork.HistoriaStatuses.GetByIdAsync(id);
        if (status == null)
            throw new KeyNotFoundException($"HistoriaStatus com ID {id} não encontrado");

        status.Nome = nome;
        status.Cor = cor;
        status.Ordem = ordem;
        status.FechaHistoria = fechaHistoria;
        status.Ativo = ativo;
        status.EmpresaId = empresaId;

        await _unitOfWork.HistoriaStatuses.UpdateAsync(status);
        await _unitOfWork.SaveChangesAsync();
        return status;
    }

    public async Task<bool> DeletarAsync(int id)
    {
        var status = await _unitOfWork.HistoriaStatuses.GetByIdAsync(id);
        if (status == null)
            throw new KeyNotFoundException($"HistoriaStatus com ID {id} não encontrado");

        await _unitOfWork.HistoriaStatuses.DeleteAsync(status);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<HistoriaStatus?> ObterPorIdAsync(int id)
    {
        return await _unitOfWork.HistoriaStatuses.GetByIdAsync(id);
    }

    public async Task<IEnumerable<HistoriaStatus>> ObterTodosAsync(int? empresaId = null)
    {
        var statuses = await _unitOfWork.HistoriaStatuses.GetAllAsync();
        if (empresaId.HasValue)
            statuses = statuses.Where(s => !s.EmpresaId.HasValue || s.EmpresaId == empresaId.Value);
        return statuses;
    }

    public async Task<IEnumerable<HistoriaStatus>> ObterPorListaIdsAsync(IEnumerable<int> ids)
    {
        if (!ids.Any()) return Enumerable.Empty<HistoriaStatus>();
        var idList = ids.Distinct().ToList();
        return await _unitOfWork.HistoriaStatuses.FindAsync(s => idList.Contains(s.Id));
    }
}
