using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class ContaPagarService : IContaPagarService
{
    private readonly IUnitOfWork _unitOfWork;

    public ContaPagarService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ContaPagar> CriarContaPagarAsync(
        int? fornecedorId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        DateTime? dataPagamento,
        ContaStatus status,
        string categoria,
        bool isRecorrente,
        int? numeroParcelas,
        int? intervaloDias,
        int empresaId,
        IEnumerable<ContaPagarItemInput>? itens = null,
        int? assinaturaId = null,
        int? afiliadoId = null)
    {
        if (!fornecedorId.HasValue && !afiliadoId.HasValue)
            throw new ArgumentException("Fornecedor ou Afiliado deve ser informado.");

        // Validar fornecedor
        if (fornecedorId.HasValue)
        {
            var fornecedor = await _unitOfWork.Pessoas.FirstOrDefaultAsync(p =>
                p.Id == fornecedorId.Value && p.EmpresaId == empresaId && p.Tipo == PessoaTipo.Fornecedor);

            if (fornecedor == null)
                throw new KeyNotFoundException("Fornecedor não encontrado para esta empresa.");
            
            if (fornecedor.Status != Status.Ativo)
                throw new PessoaInativaException(fornecedorId.Value, PessoaTipo.Fornecedor);
        }

        // Validar afiliado
        if (afiliadoId.HasValue)
        {
            var afiliado = await _unitOfWork.Afiliados.FirstOrDefaultAsync(a => a.Id == afiliadoId.Value && a.EmpresaId == empresaId);
            if (afiliado == null)
                throw new KeyNotFoundException("Afiliado não encontrado.");
                
            if (afiliado.Status != Status.Ativo)
                throw new InvalidOperationException($"Afiliado {afiliadoId} inativo.");
        }

        // Calcular valores
        var itensLista = itens?.ToList() ?? new List<ContaPagarItemInput>();
        var totalItens = itensLista.Sum(i => i.Valor);
        var valorTotal = totalItens > 0 ? totalItens : valor;

        if (valorTotal <= 0)
            throw new InvalidOperationException("Informe o valor total ou adicione itens com valores válidos.");

        var numParcelas = numeroParcelas.HasValue && numeroParcelas.Value > 0 ? numeroParcelas.Value : 1;
        var intervalo = intervaloDias.HasValue && intervaloDias.Value > 0 ? intervaloDias.Value : 30;

        // Criar conta
        var conta = new ContaPagar
        {
            EmpresaId = empresaId,
            FornecedorId = fornecedorId,
            AfiliadoId = afiliadoId,
            AssinaturaId = assinaturaId,
            Descricao = descricao,
            Valor = valorTotal,
            DataVencimento = EnsureUtc(dataVencimento),
            DataPagamento = EnsureUtcNullable(dataPagamento),
            Status = status,
            Categoria = categoria,
            IsRecorrente = isRecorrente,
            TotalParcelas = numParcelas,
            IntervaloDias = intervalo,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.ContasPagar.AddAsync(conta);
        await _unitOfWork.SaveChangesAsync();

        // Adicionar itens
        if (itensLista.Any())
        {
            foreach (var item in itensLista)
            {
                await _unitOfWork.ContaPagarItens.AddAsync(new ContaPagarItem
                {
                    EmpresaId = empresaId,
                    ContaPagarId = created.Id,
                    ProdutoId = item.ProdutoId,
                    ProdutoModuloIds = item.ProdutoModuloIds ?? new List<int>(),
                    Descricao = item.Descricao,
                    Valor = item.Valor
                });
            }
            await _unitOfWork.SaveChangesAsync();
        }

        // Gerar parcelas
        var parcelas = GerarParcelas(created, numParcelas, intervalo, valorTotal, isRecorrente);
        foreach (var parcela in parcelas)
        {
            await _unitOfWork.ContaPagarParcelas.AddAsync(parcela);
        }

        if (parcelas.Any())
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return created;
    }

    public async Task<ContaPagar> AtualizarContaPagarAsync(
        int id,
        int? fornecedorId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        DateTime? dataPagamento,
        ContaStatus status,
        string categoria,
        bool isRecorrente,
        int empresaId,
        int? afiliadoId = null)
    {
        var conta = await _unitOfWork.ContasPagar.FirstOrDefaultAsync(c =>
            c.Id == id && c.EmpresaId == empresaId);

        if (conta == null)
            throw new ContaPagarNaoEncontradaException(id);

        if (!fornecedorId.HasValue && !afiliadoId.HasValue)
            throw new ArgumentException("Fornecedor ou Afiliado deve ser informado.");

        // Validar fornecedor
        if (fornecedorId.HasValue)
        {
            var fornecedor = await _unitOfWork.Pessoas.FirstOrDefaultAsync(p =>
                p.Id == fornecedorId.Value && p.EmpresaId == empresaId && p.Tipo == PessoaTipo.Fornecedor);

            if (fornecedor == null)
                throw new KeyNotFoundException("Fornecedor não encontrado para esta empresa.");
        }

        // Validar afiliado
        if (afiliadoId.HasValue)
        {
            var afiliado = await _unitOfWork.Afiliados.FirstOrDefaultAsync(a => a.Id == afiliadoId.Value && a.EmpresaId == empresaId);
            if (afiliado == null) throw new KeyNotFoundException("Afiliado não encontrado.");
        }

        conta.FornecedorId = fornecedorId;
        conta.AfiliadoId = afiliadoId;
        conta.Descricao = descricao;
        conta.Valor = valor;
        conta.DataVencimento = EnsureUtc(dataVencimento);
        conta.DataPagamento = EnsureUtcNullable(dataPagamento);
        conta.Status = status;
        conta.Categoria = categoria;
        conta.IsRecorrente = isRecorrente;
        conta.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ContasPagar.UpdateAsync(conta);
        await _unitOfWork.SaveChangesAsync();

        return conta;
    }

    public async Task<IEnumerable<ContaPagarItem>> ObterItensPorContaIdAsync(int contaId)
    {
        return await _unitOfWork.ContaPagarItens.FindAsync(i => i.ContaPagarId == contaId);
    }

    public async Task<IEnumerable<ContaPagarItem>> ObterItensPorListaIdsAsync(IEnumerable<int> contaIds)
    {
        if (!contaIds.Any()) return Enumerable.Empty<ContaPagarItem>();
        var ids = contaIds.Distinct().ToList();
        return await _unitOfWork.ContaPagarItens.FindAsync(i => ids.Contains(i.ContaPagarId));
    }

    public async Task<IEnumerable<ContaPagarParcela>> ObterParcelasPorContaIdAsync(int contaId)
    {
        return await _unitOfWork.ContaPagarParcelas.FindAsync(p => p.ContaPagarId == contaId);
    }

    public async Task<IEnumerable<ContaPagarParcela>> ObterParcelasPorListaIdsAsync(IEnumerable<int> contaIds)
    {
        if (!contaIds.Any()) return Enumerable.Empty<ContaPagarParcela>();
        var ids = contaIds.Distinct().ToList();
        return await _unitOfWork.ContaPagarParcelas.FindAsync(p => ids.Contains(p.ContaPagarId));
    }

    public async Task<bool> DeletarContaPagarAsync(int id, int empresaId)
    {
        var conta = await _unitOfWork.ContasPagar.FirstOrDefaultAsync(c =>
            c.Id == id && c.EmpresaId == empresaId);

        if (conta == null)
            throw new ContaPagarNaoEncontradaException(id);

        await _unitOfWork.ContasPagar.DeleteAsync(conta);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<ContaPagar?> ObterContaPagarPorIdAsync(int id, int empresaId)
    {
        return await _unitOfWork.ContasPagar.FirstOrDefaultAsync(c =>
            c.Id == id && c.EmpresaId == empresaId);
    }

    public async Task<IEnumerable<ContaPagar>> ObterContasPagarAsync(
        int? empresaId = null,
        int? fornecedorId = null,
        ContaStatus? status = null,
        string? categoria = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var contas = await _unitOfWork.ContasPagar.GetAllAsync();

        if (empresaId.HasValue)
            contas = contas.Where(c => c.EmpresaId == empresaId.Value);

        if (fornecedorId.HasValue)
            contas = contas.Where(c => c.FornecedorId == fornecedorId.Value);

        if (status.HasValue)
            contas = contas.Where(c => c.Status == status.Value);

        if (!string.IsNullOrEmpty(categoria))
            contas = contas.Where(c => c.Categoria == categoria);

        if (dataInicio.HasValue)
            contas = contas.Where(c => c.DataVencimento >= dataInicio.Value);

        if (dataFim.HasValue)
            contas = contas.Where(c => c.DataVencimento <= dataFim.Value);

        return contas;
    }

    public async Task<ContaPagarParcela> AtualizarStatusParcelaAsync(
        int parcelaId,
        ContaStatus novoStatus,
        DateTime? dataPagamento,
        int empresaId)
    {
        var parcela = await _unitOfWork.ContaPagarParcelas.FirstOrDefaultAsync(p =>
            p.Id == parcelaId && p.EmpresaId == empresaId);

        if (parcela == null)
            throw new KeyNotFoundException("Parcela não encontrada.");

        parcela.Status = novoStatus;
        parcela.DataPagamento = EnsureUtcNullable(dataPagamento);

        await _unitOfWork.ContaPagarParcelas.UpdateAsync(parcela);
        await _unitOfWork.SaveChangesAsync();

        return parcela;
    }

    private static List<ContaPagarParcela> GerarParcelas(
        ContaPagar conta,
        int numeroParcelas,
        int intervaloDias,
        decimal valorTotal,
        bool isRecorrente)
    {
        var parcelas = new List<ContaPagarParcela>();
        var valorBase = isRecorrente
            ? valorTotal
            : Math.Round(valorTotal / numeroParcelas, 2, MidpointRounding.AwayFromZero);
        decimal acumulado = 0;

        for (int i = 1; i <= numeroParcelas; i++)
        {
            var valor = isRecorrente
                ? valorTotal
                : (i == numeroParcelas ? valorTotal - acumulado : valorBase);

            if (!isRecorrente)
            {
                acumulado += valor;
            }

            var vencimento = conta.DataVencimento.AddDays(intervaloDias * (i - 1));

            parcelas.Add(new ContaPagarParcela
            {
                EmpresaId = conta.EmpresaId,
                ContaPagarId = conta.Id,
                Numero = i,
                Valor = valor,
                DataVencimento = vencimento,
                Status = ContaStatus.Pendente
            });
        }

        return parcelas;
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static DateTime? EnsureUtcNullable(DateTime? value)
    {
        return value.HasValue ? EnsureUtc(value.Value) : null;
    }
}
