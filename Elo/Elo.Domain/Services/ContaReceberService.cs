using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class ContaReceberService : IContaReceberService
{
    private readonly IUnitOfWork _unitOfWork;

    public ContaReceberService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ContaReceber> CriarContaReceberAsync(
        int clienteId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        DateTime? dataRecebimento,
        ContaStatus status,
        FormaPagamento formaPagamento,
        int? numeroParcelas,
        int? intervaloDias,
        int empresaId,
        IEnumerable<ContaReceberItemInput>? itens = null,
        bool isRecorrente = false,
        int? assinaturaId = null)
    {
        // Validar cliente
        var cliente = await _unitOfWork.Pessoas.FirstOrDefaultAsync(p =>
            p.Id == clienteId && p.EmpresaId == empresaId && p.Tipo == PessoaTipo.Cliente);

        if (cliente == null)
            throw new KeyNotFoundException("Cliente não encontrado para esta empresa.");
        
        if (cliente.Status != Status.Ativo)
            throw new PessoaInativaException(clienteId, PessoaTipo.Cliente);

        // Calcular valores
        var itensLista = itens?.ToList() ?? new List<ContaReceberItemInput>();
        var totalItens = itensLista.Sum(i => i.Valor);
        var valorTotal = totalItens > 0 ? totalItens : valor;

        if (valorTotal <= 0)
            throw new InvalidOperationException("Informe o valor total ou adicione itens com valores válidos.");

        var numParcelas = numeroParcelas.HasValue && numeroParcelas.Value > 0 ? numeroParcelas.Value : 1;
        var intervalo = intervaloDias.HasValue && intervaloDias.Value > 0 ? intervaloDias.Value : 30;

        // Criar conta
        var conta = new ContaReceber
        {
            EmpresaId = empresaId,
            ClienteId = clienteId,
            AssinaturaId = assinaturaId,
            Descricao = descricao,
            Valor = valorTotal,
            DataVencimento = EnsureUtc(dataVencimento),
            DataRecebimento = EnsureUtcNullable(dataRecebimento),
            Status = status,
            FormaPagamento = formaPagamento,
            IsRecorrente = isRecorrente,
            TotalParcelas = numParcelas,
            IntervaloDias = intervalo,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.ContasReceber.AddAsync(conta);
        await _unitOfWork.SaveChangesAsync();

        // Adicionar itens
        if (itensLista.Any())
        {
            foreach (var item in itensLista)
            {
                await _unitOfWork.ContaReceberItens.AddAsync(new ContaReceberItem
                {
                    EmpresaId = empresaId,
                    ContaReceberId = created.Id,
                    ProdutoId = null,
                    ProdutoModuloIds = new List<int>(),
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
            await _unitOfWork.ContaReceberParcelas.AddAsync(parcela);
        }

        if (parcelas.Any())
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return created;
    }

    public async Task<ContaReceber> AtualizarContaReceberAsync(
        int id,
        int clienteId,
        string descricao,
        decimal valor,
        DateTime dataVencimento,
        DateTime? dataRecebimento,
        ContaStatus status,
        FormaPagamento formaPagamento,
        int empresaId)
    {
        var conta = await _unitOfWork.ContasReceber.FirstOrDefaultAsync(c =>
            c.Id == id && c.EmpresaId == empresaId);

        if (conta == null)
            throw new ContaReceberNaoEncontradaException(id);

        // Validar cliente
        var cliente = await _unitOfWork.Pessoas.FirstOrDefaultAsync(p =>
            p.Id == clienteId && p.EmpresaId == empresaId && p.Tipo == PessoaTipo.Cliente);

        if (cliente == null)
            throw new KeyNotFoundException("Cliente não encontrado para esta empresa.");

        // Atualizar conta
        conta.ClienteId = clienteId;
        conta.Descricao = descricao;
        conta.Valor = valor;
        conta.DataVencimento = EnsureUtc(dataVencimento);
        conta.DataRecebimento = EnsureUtcNullable(dataRecebimento);
        conta.Status = status;
        conta.FormaPagamento = formaPagamento;
        conta.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ContasReceber.UpdateAsync(conta);

        // Atualizar parcelas
        var parcelas = (await _unitOfWork.ContaReceberParcelas.FindAsync(p => p.ContaReceberId == conta.Id)).ToList();
        if (parcelas.Any())
        {
            foreach (var parcela in parcelas)
            {
                parcela.Status = conta.Status;
                parcela.DataRecebimento = conta.Status == ContaStatus.Pago
                    ? conta.DataRecebimento ?? parcela.DataRecebimento
                    : null;
                await _unitOfWork.ContaReceberParcelas.UpdateAsync(parcela);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return conta;
    }

    public async Task<IEnumerable<ContaReceberItem>> ObterItensPorContaIdAsync(int contaId)
    {
        return await _unitOfWork.ContaReceberItens.FindAsync(i => i.ContaReceberId == contaId);
    }

    public async Task<IEnumerable<ContaReceberItem>> ObterItensPorListaIdsAsync(IEnumerable<int> contaIds)
    {
        if (!contaIds.Any()) return Enumerable.Empty<ContaReceberItem>();
        var ids = contaIds.Distinct().ToList();
        return await _unitOfWork.ContaReceberItens.FindAsync(i => ids.Contains(i.ContaReceberId));
    }

    public async Task<IEnumerable<ContaReceberParcela>> ObterParcelasPorContaIdAsync(int contaId)
    {
        return await _unitOfWork.ContaReceberParcelas.FindAsync(p => p.ContaReceberId == contaId);
    }

    public async Task<IEnumerable<ContaReceberParcela>> ObterParcelasPorListaIdsAsync(IEnumerable<int> contaIds)
    {
        if (!contaIds.Any()) return Enumerable.Empty<ContaReceberParcela>();
        var ids = contaIds.Distinct().ToList();
        return await _unitOfWork.ContaReceberParcelas.FindAsync(p => ids.Contains(p.ContaReceberId));
    }

    public async Task<bool> DeletarContaReceberAsync(int id, int empresaId)
    {
        var conta = await _unitOfWork.ContasReceber.FirstOrDefaultAsync(c =>
            c.Id == id && c.EmpresaId == empresaId);

        if (conta == null)
            throw new ContaReceberNaoEncontradaException(id);

        await _unitOfWork.ContasReceber.DeleteAsync(conta);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<ContaReceber?> ObterContaReceberPorIdAsync(int id, int empresaId)
    {
        var conta = await _unitOfWork.ContasReceber.FirstOrDefaultAsync(c =>
            c.Id == id && c.EmpresaId == empresaId);

        if (conta != null && conta.Status == ContaStatus.Pendente && conta.DataVencimento.Date < DateTime.UtcNow.Date)
        {
            var configs = await _unitOfWork.EmpresaConfiguracoes.FindAsync(c => c.EmpresaId == empresaId);
            var config = configs.FirstOrDefault();

            if (config != null)
            {
                decimal acrescimo = 0;

                // Mora (Penalty) - One time
                if (config.MoraValor > 0)
                {
                    if (config.MoraTipo == TipoValor.Fixo)
                        acrescimo += config.MoraValor;
                    else // Porcentagem
                        acrescimo += conta.Valor * (config.MoraValor / 100m);
                }

                // Juros (Interest) - Time based
                var diasAtraso = (DateTime.UtcNow.Date - conta.DataVencimento.Date).Days;
                if (config.JurosValor > 0 && diasAtraso > 0)
                {
                    if (config.JurosTipo == TipoValor.Fixo)
                    {
                        // Fixed amount per day
                        acrescimo += config.JurosValor * diasAtraso;
                    }
                    else // Porcentagem (Monthly rate)
                    {
                        var jurosDiario = (config.JurosValor / 30m) / 100m;
                        acrescimo += conta.Valor * jurosDiario * diasAtraso;
                    }
                }

                conta.Valor += Math.Round(acrescimo, 2);
            }
        }

        return conta;
    }

    public async Task<IEnumerable<ContaReceber>> ObterContasReceberAsync(
        int? empresaId = null,
        int? clienteId = null,
        ContaStatus? status = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var contas = await _unitOfWork.ContasReceber.GetAllAsync();

        if (empresaId.HasValue)
            contas = contas.Where(c => c.EmpresaId == empresaId.Value);

        if (clienteId.HasValue)
            contas = contas.Where(c => c.ClienteId == clienteId.Value);

        if (status.HasValue)
            contas = contas.Where(c => c.Status == status.Value);

        if (dataInicio.HasValue)
            contas = contas.Where(c => c.DataVencimento >= dataInicio.Value);

        if (dataFim.HasValue)
            contas = contas.Where(c => c.DataVencimento <= dataFim.Value);

        // Calcular Juros e Mora se empresa for informada
        if (empresaId.HasValue)
        {
            var overdue = contas.Where(c => c.Status == ContaStatus.Pendente && c.DataVencimento.Date < DateTime.UtcNow.Date).ToList();
            if (overdue.Any())
            {
                var configs = await _unitOfWork.EmpresaConfiguracoes.FindAsync(c => c.EmpresaId == empresaId.Value);
                var config = configs.FirstOrDefault();

                if (config != null)
                {
                    foreach (var conta in overdue)
                    {
                        decimal acrescimo = 0;

                        if (config.MoraValor > 0)
                        {
                            if (config.MoraTipo == TipoValor.Fixo)
                                acrescimo += config.MoraValor;
                            else
                                acrescimo += conta.Valor * (config.MoraValor / 100m);
                        }

                        var diasAtraso = (DateTime.UtcNow.Date - conta.DataVencimento.Date).Days;
                        if (config.JurosValor > 0 && diasAtraso > 0)
                        {
                            if (config.JurosTipo == TipoValor.Fixo)
                                acrescimo += config.JurosValor * diasAtraso;
                            else
                            {
                                var jurosDiario = (config.JurosValor / 30m) / 100m;
                                acrescimo += conta.Valor * jurosDiario * diasAtraso;
                            }
                        }

                        conta.Valor += Math.Round(acrescimo, 2);
                    }
                }
            }
        }

        return contas;
    }

    public async Task<ContaReceberParcela> AtualizarStatusParcelaAsync(
        int parcelaId,
        ContaStatus novoStatus,
        DateTime? dataRecebimento,
        int empresaId)
    {
        var parcela = await _unitOfWork.ContaReceberParcelas.FirstOrDefaultAsync(p =>
            p.Id == parcelaId && p.EmpresaId == empresaId);

        if (parcela == null)
            throw new KeyNotFoundException("Parcela não encontrada.");

        parcela.Status = novoStatus;
        parcela.DataRecebimento = EnsureUtcNullable(dataRecebimento);

        await _unitOfWork.ContaReceberParcelas.UpdateAsync(parcela);
        await _unitOfWork.SaveChangesAsync();

        return parcela;
    }

    private static List<ContaReceberParcela> GerarParcelas(
        ContaReceber conta,
        int numeroParcelas,
        int intervaloDias,
        decimal valorTotal,
        bool isRecorrente)
    {
        var parcelas = new List<ContaReceberParcela>();
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
                acumulado += valor;
            
            var vencimento = conta.DataVencimento.AddDays(intervaloDias * (i - 1));

            parcelas.Add(new ContaReceberParcela
            {
                EmpresaId = conta.EmpresaId,
                ContaReceberId = conta.Id,
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
