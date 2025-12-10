using Elo.Domain.Entities;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class TicketTipoService : ITicketTipoService
{
    private readonly IUnitOfWork _unitOfWork;

    public TicketTipoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketTipo> CriarAsync(string nome, string? descricao, int ordem, bool ativo, int? empresaId)
    {
        var tipo = new TicketTipo
        {
            Nome = nome,
            Descricao = descricao,
            Ordem = ordem,
            Ativo = ativo,
            EmpresaId = empresaId
        };

        var created = await _unitOfWork.TicketTipos.AddAsync(tipo);
        await _unitOfWork.SaveChangesAsync();
        return created;
    }

    public async Task<TicketTipo> AtualizarAsync(int id, string nome, string? descricao, int ordem, bool ativo, int? empresaId)
    {
        var tipo = await _unitOfWork.TicketTipos.GetByIdAsync(id);
        if (tipo == null)
            throw new KeyNotFoundException($"TicketTipo com ID {id} não encontrado");

        tipo.Nome = nome;
        tipo.Descricao = descricao;
        tipo.Ordem = ordem;
        tipo.Ativo = ativo;
        tipo.EmpresaId = empresaId;

        await _unitOfWork.TicketTipos.UpdateAsync(tipo);
        await _unitOfWork.SaveChangesAsync();
        return tipo;
    }

    public async Task<bool> DeletarAsync(int id)
    {
        var tipo = await _unitOfWork.TicketTipos.GetByIdAsync(id);
        if (tipo == null)
            throw new KeyNotFoundException($"TicketTipo com ID {id} não encontrado");

        await _unitOfWork.TicketTipos.DeleteAsync(tipo);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<TicketTipo?> ObterPorIdAsync(int id)
    {
        return await _unitOfWork.TicketTipos.GetByIdAsync(id);
    }

    public async Task<IEnumerable<TicketTipo>> ObterTodosAsync(int? empresaId = null)
    {
        var tipos = await _unitOfWork.TicketTipos.GetAllAsync();
        if (empresaId.HasValue)
            tipos = tipos.Where(t => !t.EmpresaId.HasValue || t.EmpresaId == empresaId.Value);
        return tipos;
    }
}
