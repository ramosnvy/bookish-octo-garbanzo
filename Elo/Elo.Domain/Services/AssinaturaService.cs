using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Elo.Domain.Services;

public class AssinaturaService : IAssinaturaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHistoriaService _historiaService;
    private readonly IContaReceberService _contaReceberService;
    private readonly IContaPagarService _contaPagarService;

    public AssinaturaService(
        IUnitOfWork unitOfWork,
        IHistoriaService historiaService,
        IContaReceberService contaReceberService,
        IContaPagarService contaPagarService)
    {
        _unitOfWork = unitOfWork;
        _historiaService = historiaService;
        _contaReceberService = contaReceberService;
        _contaPagarService = contaPagarService;
    }

    public async Task<Assinatura> CriarAssinaturaAsync(
        int empresaId,
        int clienteId,
        bool isRecorrente,
        int? intervaloDias,
        int? recorrenciaQtde,
        DateTime dataInicio,
        DateTime? dataFim,
        bool gerarFinanceiro,
        bool gerarImplantacao,
        List<AssinaturaItemInput> itens,
        FormaPagamento? formaPagamento,
        int? afiliadoId = null,
        int? usuarioCriadorId = null)
    {
        // Validar cliente
        var cliente = await _unitOfWork.Pessoas.GetByIdAsync(clienteId);
        if (cliente == null || cliente.EmpresaId != empresaId)
            throw new KeyNotFoundException("Cliente não encontrado.");
        
        if (cliente.Tipo != PessoaTipo.Cliente)
            throw new InvalidOperationException("A pessoa informada não é um cliente.");
        
        if (cliente.Status != Status.Ativo)
            throw new PessoaInativaException(clienteId, PessoaTipo.Cliente);

        // Validar produtos e módulos dos itens
        if (itens != null && itens.Any())
        {
            foreach (var item in itens)
            {
                var produto = await _unitOfWork.Produtos.GetByIdAsync(item.ProdutoId);
                if (produto == null || produto.EmpresaId != empresaId)
                    throw new KeyNotFoundException($"Produto {item.ProdutoId} não encontrado.");
                
                if (!produto.Ativo)
                    throw new ProdutoInativoException(item.ProdutoId);

                if (item.ProdutoModuloId.HasValue)
                {
                    var modulo = await _unitOfWork.ProdutoModulos.GetByIdAsync(item.ProdutoModuloId.Value);
                    if (modulo == null || modulo.ProdutoId != item.ProdutoId)
                        throw new KeyNotFoundException($"Módulo {item.ProdutoModuloId.Value} não encontrado ou não pertence ao produto.");
                    
                    if (!modulo.Ativo)
                        throw new ModuloInativoException(item.ProdutoModuloId.Value);
                }
            }
        }

        // Validar Forma de Pagamento
        if (gerarFinanceiro && !formaPagamento.HasValue)
            throw new InvalidOperationException("A forma de pagamento é obrigatória para gerar o financeiro.");

        if (formaPagamento.HasValue)
        {
            var metodoValido = await _unitOfWork.EmpresaFormasPagamento
                .FirstOrDefaultAsync(efp => efp.EmpresaId == empresaId && efp.FormaPagamento == formaPagamento.Value && efp.Ativo);

            if (metodoValido == null)
                throw new InvalidOperationException($"A forma de pagamento {formaPagamento} não está ativa para esta empresa.");
        }

        // 1. Create Assinatura Entity
        var assinatura = new Assinatura
        {
            EmpresaId = empresaId,
            ClienteId = clienteId,
            IsRecorrente = isRecorrente,
            IntervaloDias = intervaloDias,
            RecorrenciaQtde = recorrenciaQtde,
            DataInicio = dataInicio,
            DataFim = dataFim,
            GerarFinanceiro = gerarFinanceiro,
            GerarImplantacao = gerarImplantacao,
            AfiliadoId = afiliadoId,
            FormaPagamento = formaPagamento,
            Ativo = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdAssinatura = await _unitOfWork.Assinaturas.AddAsync(assinatura);
        await _unitOfWork.SaveChangesAsync();

        // 2. Add Items
        if (itens != null && itens.Any())
        {
            foreach (var item in itens)
            {
                await _unitOfWork.AssinaturaItens.AddAsync(new AssinaturaItem
                {
                    AssinaturaId = createdAssinatura.Id,
                    ProdutoId = item.ProdutoId,
                    ProdutoModuloId = item.ProdutoModuloId
                });
            }
            await _unitOfWork.SaveChangesAsync();
        }

        var produtosInfo = new List<(string Nome, decimal Valor)>();
        var contasPagarInfo = new List<(int FornecedorId, string Descricao, decimal Valor, int? ProdutoId, List<int>? ModuloIds)>();

        if (gerarFinanceiro && itens != null && itens.Any())
        {
            var moduloCache = new Dictionary<int, ProdutoModulo?>();

            async Task<ProdutoModulo?> GetModuloAsync(int moduloId)
            {
                if (moduloCache.TryGetValue(moduloId, out var cached))
                {
                    return cached;
                }

                var modulo = await _unitOfWork.ProdutoModulos.GetByIdAsync(moduloId);
                moduloCache[moduloId] = modulo;
                return modulo;
            }

            var itensPorProduto = itens.GroupBy(i => i.ProdutoId);
            foreach (var grupo in itensPorProduto)
            {
                var produto = await _unitOfWork.Produtos.GetByIdAsync(grupo.Key);
                if (produto == null || produto.EmpresaId != empresaId)
                    continue;

                var baseEntries = grupo.Where(i => !i.ProdutoModuloId.HasValue).ToList();
                var moduleEntries = grupo.Where(i => i.ProdutoModuloId.HasValue).ToList();

                foreach (var _ in baseEntries)
                {
                    produtosInfo.Add((produto.Nome, produto.ValorRevenda));
                }

                if (!baseEntries.Any() && moduleEntries.Any())
                {
                    produtosInfo.Add((produto.Nome, produto.ValorRevenda));
                }

                if (produto.FornecedorId.HasValue && produto.FornecedorId.Value > 0)
                {
                    var fornecedorId = produto.FornecedorId.Value;
                    if (baseEntries.Any())
                    {
                        foreach (var _ in baseEntries)
                        {
                            contasPagarInfo.Add((fornecedorId, produto.Nome, produto.ValorCusto, produto.Id, null));
                        }
                    }
                    else if (moduleEntries.Any())
                    {
                        contasPagarInfo.Add((fornecedorId, produto.Nome, produto.ValorCusto, produto.Id, null));
                    }
                }

                foreach (var moduleEntry in moduleEntries)
                {
                    var modulo = await GetModuloAsync(moduleEntry.ProdutoModuloId!.Value);
                    if (modulo == null)
                        continue;

                    produtosInfo.Add((modulo.Nome, modulo.ValorAdicional));

                    if (produto.FornecedorId.HasValue && produto.FornecedorId.Value > 0 && modulo.CustoAdicional > 0)
                    {
                        contasPagarInfo.Add((produto.FornecedorId.Value, $"{modulo.Nome} (Módulo de {produto.Nome})", modulo.CustoAdicional, produto.Id, new List<int> { modulo.Id }));
                    }
                }
            }
        }

        // 3. Gerar Implantação (Kanban)
        if (gerarImplantacao)
        {
             var tipos = await _unitOfWork.HistoriaTipos.GetAllAsync();
             var implantacaoTipo = tipos.FirstOrDefault(t => t.Nome.Contains("Implantação", StringComparison.OrdinalIgnoreCase)) ?? tipos.FirstOrDefault();
             
             var statuses = await _unitOfWork.HistoriaStatuses.GetAllAsync();
             var initialStatus = statuses
                 .Where(s => s.Ativo && (!s.EmpresaId.HasValue || s.EmpresaId == empresaId))
                 .OrderBy(s => s.Ordem)
                 .FirstOrDefault();

             if (implantacaoTipo != null && initialStatus != null)
             {
                 var mainProduct = itens?.FirstOrDefault()?.ProdutoId ?? 0;
                 if (mainProduct == 0)
                 {
                     // Fallback if no product was selected, although subscription usually has one.
                     // We can try to get from db or fail. 
                     // Or just skip product specific logic.
                 }

                 // Group items for Historia Inputs
                 var historiaProds = new List<HistoriaProdutoInput>();
                 if (itens != null)
                 {
                    var grouped = itens.GroupBy(i => i.ProdutoId);
                    foreach(var g in grouped)
                    {
                        var modIds = g.Where(x => x.ProdutoModuloId.HasValue).Select(x => x.ProdutoModuloId!.Value).Distinct().ToList();
                        historiaProds.Add(new HistoriaProdutoInput(g.Key, modIds));
                    }
                 }

                 await _historiaService.CriarHistoriaAsync(
                     clienteId,
                     mainProduct,
                     initialStatus.Id,
                     implantacaoTipo.Id,
                     null, // No responsible yet
                     $"Implantação gerada automaticamente pela assinatura #{createdAssinatura.Id}",
                     null, // Previsao
                     empresaId,
                     historiaProds,
                     usuarioCriadorId
                 );
             }
        }

        // 4. Gerar Financeiro (ContasReceber)
        if (gerarFinanceiro && produtosInfo.Any())
        {
            var total = produtosInfo.Sum(x => x.Valor);
            var crItens = produtosInfo.Select(x => new ContaReceberItemInput(x.Nome, x.Valor)).ToList();

            int qtde = isRecorrente ? (recorrenciaQtde ?? 1) : 1;
            int intervalo = (isRecorrente && intervaloDias.HasValue) ? intervaloDias.Value : 30;

            for (int i = 0; i < qtde; i++)
            {
                var vencimentoBase = dataInicio.AddDays(intervalo * i);
                var vencimentoReceber = vencimentoBase;

                if (cliente.ServicoPagamentoTipo == ServicoPagamentoTipo.PosPago)
                {
                    vencimentoReceber = vencimentoBase.AddDays(cliente.PrazoPagamentoDias);
                }
                else
                {
                    vencimentoReceber = vencimentoBase.AddDays(-cliente.PrazoPagamentoDias);
                }

                var descricao = $"Assinatura #{createdAssinatura.Id}";
                if (qtde > 1)
                {
                    descricao += $" - {i + 1}/{qtde}";
                }

                await _contaReceberService.CriarContaReceberAsync(
                    clienteId,
                    descricao,
                    total,
                    vencimentoReceber,
                    null, // Data Recebimento
                    ContaStatus.Pendente,
                    formaPagamento!.Value,
                    1, // Parcelas por conta (cada conta é única)
                    null, // Intervalo (não aplica, pois é conta única)
                    empresaId,
                    crItens,
                    false, // Não é recorrente no sentido de 'uma conta com várias parcelas'
                    createdAssinatura.Id // AssinaturaId
                );
            }
        }

        // 5. Gerar Financeiro (ContasPagar para Fornecedores)
        if (gerarFinanceiro && contasPagarInfo.Any())
        {
            var porFornecedor = contasPagarInfo.GroupBy(x => x.FornecedorId);
            foreach (var group in porFornecedor)
            {
                var fornecedorId = group.Key;
                var fornecedor = await _unitOfWork.Pessoas.GetByIdAsync(fornecedorId);

                if (fornecedor != null)
                {
                    var totalCusto = group.Sum(x => x.Valor);
                    var cpItens = group.Select(x => new ContaPagarItemInput(x.Descricao, x.Valor, x.ProdutoId, x.ModuloIds)).ToList();

                    int qtde = isRecorrente ? (recorrenciaQtde ?? 1) : 1;
                    int intervalo = (isRecorrente && intervaloDias.HasValue) ? intervaloDias.Value : 30;

                    for (int i = 0; i < qtde; i++)
                    {
                        var vencimentoBase = dataInicio.AddDays(intervalo * i);
                        var vencimento = vencimentoBase;

                        if (fornecedor.ServicoPagamentoTipo == ServicoPagamentoTipo.PosPago)
                        {
                            vencimento = vencimentoBase.AddDays(fornecedor.PrazoPagamentoDias);
                        }
                        else
                        {
                            vencimento = vencimentoBase.AddDays(-fornecedor.PrazoPagamentoDias);
                        }

                        var descricao = $"Custo Assinatura #{createdAssinatura.Id}";
                        if (qtde > 1)
                        {
                            descricao += $" - {i + 1}/{qtde}";
                        }

                        await _contaPagarService.CriarContaPagarAsync(
                            fornecedorId,
                            descricao,
                            totalCusto,
                            vencimento,
                            null, // Data Pagamento
                            ContaStatus.Pendente,
                            "Custos de Assinatura", // Categoria
                            false, // Não é recorrente (é conta única)
                            1, // Parcelas
                            null, // Intervalo
                            empresaId,
                            cpItens,
                            createdAssinatura.Id // AssinaturaId
                        );
                    }
                }
            }
        }

        // 6. Gerar Financeiro (ContasPagar para Afiliado)
        if (gerarFinanceiro && afiliadoId.HasValue)
        {
            var afiliado = await _unitOfWork.Afiliados.GetByIdAsync(afiliadoId.Value);
            var configs = await _unitOfWork.EmpresaConfiguracoes.FindAsync(c => c.EmpresaId == empresaId);
            var config = configs.FirstOrDefault();
            
            if (afiliado != null)
            {
                 var totalAssinatura = produtosInfo.Sum(x => x.Valor);
                 if (totalAssinatura > 0)
                 {
                     var valorComissao = totalAssinatura * (afiliado.Porcentagem / 100m);
                     var diaPagamento = config?.DiaPagamentoAfiliado ?? 1; // Default
                     if (diaPagamento <= 0) diaPagamento = 1;

                     int qtde = isRecorrente ? (recorrenciaQtde ?? 1) : 1;
                     int intervalo = (isRecorrente && intervaloDias.HasValue) ? intervaloDias.Value : 30;

                     for (int i = 0; i < qtde; i++)
                     {
                         var dataBase = dataInicio.AddDays(intervalo * i);
                         
                         // Determine Payment Date
                         int year = dataBase.Year;
                         int month = dataBase.Month;
                         int daysInMonth = DateTime.DaysInMonth(year, month);
                         int paymentDay = diaPagamento > daysInMonth ? daysInMonth : diaPagamento;
                         
                         var dataPrevista = new DateTime(year, month, paymentDay);
                         
                         // If computed date is before or same as cycle start, maybe push to next month? 
                         // Usually commissions are paid after value is earned.
                         if (dataPrevista <= dataBase)
                         {
                             dataPrevista = dataPrevista.AddMonths(1);
                             // Adjust day again for new month
                             daysInMonth = DateTime.DaysInMonth(dataPrevista.Year, dataPrevista.Month);
                             if (diaPagamento > daysInMonth) 
                                 dataPrevista = new DateTime(dataPrevista.Year, dataPrevista.Month, daysInMonth);
                             else
                                 dataPrevista = new DateTime(dataPrevista.Year, dataPrevista.Month, diaPagamento);
                         }
                         
                         var descricao = $"Comissão Afiliado - Assinatura #{createdAssinatura.Id}";
                         if (qtde > 1) descricao += $" - {i + 1}/{qtde}";
                         
                         await _contaPagarService.CriarContaPagarAsync(
                             null, // FornecedorId nullable
                             descricao,
                             valorComissao,
                             dataPrevista, // Vencimento
                             null, // Data Pagamento
                             ContaStatus.Pendente,
                             "Comissão de Afiliados",
                             false, 
                             1,
                             null,
                             empresaId,
                             null, // Itens
                             createdAssinatura.Id,
                             afiliadoId // AfiliadoId
                         );
                     }
                 }
            }
        }

        return createdAssinatura;
    }

    public async Task<IEnumerable<Assinatura>> ObterAssinaturasAsync(int empresaId)
    {
        return await _unitOfWork.Assinaturas.FindAsync(a => a.EmpresaId == empresaId);
    }

    public async Task<IEnumerable<AssinaturaItem>> ObterItensPorAssinaturaIdsAsync(IEnumerable<int> assinaturaIds)
    {
        if (!assinaturaIds.Any()) return Enumerable.Empty<AssinaturaItem>();
        var ids = assinaturaIds.Distinct().ToList();
        return await _unitOfWork.AssinaturaItens.FindAsync(i => ids.Contains(i.AssinaturaId));
    }

    public async Task CancelarAssinaturaAsync(int assinaturaId, int empresaId)
    {
        var assinatura = await _unitOfWork.Assinaturas.FirstOrDefaultAsync(a => 
            a.Id == assinaturaId && a.EmpresaId == empresaId);
        
        if (assinatura == null)
            throw new KeyNotFoundException("Assinatura não encontrada.");
        
        if (!assinatura.Ativo)
            throw new InvalidOperationException("Assinatura já está cancelada.");

        // Marcar assinatura como inativa
        assinatura.Ativo = false;
        assinatura.DataFim = DateTime.UtcNow;
        assinatura.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Assinaturas.UpdateAsync(assinatura);

        // Calcular período utilizado
        var hoje = DateTime.UtcNow.Date;
        var inicioPeriodo = assinatura.DataInicio.Date;
        var intervaloDias = assinatura.IntervaloDias ?? 30;
        
        // Calcular quantos períodos completos foram utilizados
        var diasUtilizados = (hoje - inicioPeriodo).Days;
        var periodosUtilizados = (int)Math.Ceiling((double)diasUtilizados / intervaloDias);
        if (periodosUtilizados < 1) periodosUtilizados = 1; // Mínimo de 1 período deve ser cobrado

        // Buscar todas as contas a receber relacionadas à assinatura
        var contasReceber = await _unitOfWork.ContasReceber.FindAsync(c => 
            c.AssinaturaId == assinaturaId && c.EmpresaId == empresaId);
        
        var listaContasReceber = contasReceber.OrderBy(c => c.DataVencimento).ToList();
        
        // Cancelar contas futuras (após o período utilizado)
        for (int i = 0; i < listaContasReceber.Count; i++)
        {
            var conta = listaContasReceber[i];
            
            // Se a conta é de um período futuro não utilizado e está pendente
            if (i >= periodosUtilizados && conta.Status == ContaStatus.Pendente)
            {
                conta.Status = ContaStatus.Cancelado;
                conta.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.ContasReceber.UpdateAsync(conta);
                
                // Cancelar parcelas relacionadas
                var parcelas = await _unitOfWork.ContaReceberParcelas.FindAsync(p => p.ContaReceberId == conta.Id);
                foreach (var parcela in parcelas)
                {
                    if (parcela.Status == ContaStatus.Pendente)
                    {
                        parcela.Status = ContaStatus.Cancelado;
                        await _unitOfWork.ContaReceberParcelas.UpdateAsync(parcela);
                    }
                }
            }
        }

        // Buscar todas as contas a pagar relacionadas à assinatura
        var contasPagar = await _unitOfWork.ContasPagar.FindAsync(c => 
            c.AssinaturaId == assinaturaId && c.EmpresaId == empresaId);
        
        var listaContasPagar = contasPagar.OrderBy(c => c.DataVencimento).ToList();
        
        // Cancelar contas a pagar futuras (após o período utilizado)
        for (int i = 0; i < listaContasPagar.Count; i++)
        {
            var conta = listaContasPagar[i];
            
            // Se a conta é de um período futuro não utilizado e está pendente
            if (i >= periodosUtilizados && conta.Status == ContaStatus.Pendente)
            {
                conta.Status = ContaStatus.Cancelado;
                conta.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.ContasPagar.UpdateAsync(conta);
                
                // Cancelar parcelas relacionadas
                var parcelas = await _unitOfWork.ContaPagarParcelas.FindAsync(p => p.ContaPagarId == conta.Id);
                foreach (var parcela in parcelas)
                {
                    if (parcela.Status == ContaStatus.Pendente)
                    {
                        parcela.Status = ContaStatus.Cancelado;
                        await _unitOfWork.ContaPagarParcelas.UpdateAsync(parcela);
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
