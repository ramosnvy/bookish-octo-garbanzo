using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class EmpresaFormaPagamentoService : IEmpresaFormaPagamentoService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmpresaFormaPagamentoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmpresaFormaPagamento> CriarFormaPagamentoAsync(int empresaId, FormaPagamento formaPagamento, string nome, bool aVista)
    {
        // Validar se a empresa existe
        var empresa = await _unitOfWork.Empresas.GetByIdAsync(empresaId);
        if (empresa == null)
            throw new KeyNotFoundException("Empresa não encontrada.");

        if (!empresa.Ativo)
            throw new EmpresaInativaException(empresaId);

        // Verificar se a forma de pagamento já está cadastrada para esta empresa
        var formasPagamento = await _unitOfWork.EmpresaFormasPagamento.FindAsync(
            fp => fp.EmpresaId == empresaId && fp.FormaPagamento == formaPagamento);

        var formaExistente = formasPagamento.FirstOrDefault();
        if (formaExistente != null)
        {
            // Se já existe mas está inativa, reativar e atualizar
            if (!formaExistente.Ativo)
            {
                formaExistente.Ativo = true;
                formaExistente.Nome = nome;
                formaExistente.AVista = aVista;
                formaExistente.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.EmpresaFormasPagamento.UpdateAsync(formaExistente);
                await _unitOfWork.SaveChangesAsync();
                return formaExistente;
            }
            
            throw new InvalidOperationException("Esta forma de pagamento já está cadastrada para a empresa.");
        }

        // Criar nova forma de pagamento
        var novaFormaPagamento = new EmpresaFormaPagamento
        {
            EmpresaId = empresaId,
            FormaPagamento = formaPagamento,
            Nome = nome,
            AVista = aVista,
            Ativo = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.EmpresaFormasPagamento.AddAsync(novaFormaPagamento);
        await _unitOfWork.SaveChangesAsync();

        return created;
    }

    public async Task<IEnumerable<EmpresaFormaPagamento>> ObterFormasPagamentoPorEmpresaAsync(int empresaId, bool apenasAtivos = true)
    {
        var query = await _unitOfWork.EmpresaFormasPagamento.FindAsync(fp => fp.EmpresaId == empresaId);

        if (apenasAtivos)
        {
            query = query.Where(fp => fp.Ativo);
        }

        return query.OrderBy(fp => fp.FormaPagamento);
    }

    public async Task<EmpresaFormaPagamento?> ObterFormaPagamentoPorIdAsync(int id, int empresaId)
    {
        var formaPagamento = await _unitOfWork.EmpresaFormasPagamento.GetByIdAsync(id);
        
        if (formaPagamento == null || formaPagamento.EmpresaId != empresaId)
            return null;

        return formaPagamento;
    }

    public async Task AtualizarStatusFormaPagamentoAsync(int id, int empresaId, bool ativo)
    {
        var formaPagamento = await ObterFormaPagamentoPorIdAsync(id, empresaId);
        
        if (formaPagamento == null)
            throw new KeyNotFoundException("Forma de pagamento não encontrada.");

        formaPagamento.Ativo = ativo;
        formaPagamento.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.EmpresaFormasPagamento.UpdateAsync(formaPagamento);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeletarFormaPagamentoAsync(int id, int empresaId)
    {
        var formaPagamento = await ObterFormaPagamentoPorIdAsync(id, empresaId);
        
        if (formaPagamento == null)
            throw new KeyNotFoundException("Forma de pagamento não encontrada.");

        // Soft delete - apenas marca como inativo
        formaPagamento.Ativo = false;
        formaPagamento.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.EmpresaFormasPagamento.UpdateAsync(formaPagamento);
        await _unitOfWork.SaveChangesAsync();
    }
}
