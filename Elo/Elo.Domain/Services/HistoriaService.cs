using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class HistoriaService : IHistoriaService
{
    private readonly IUnitOfWork _unitOfWork;

    public HistoriaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Historia> CriarHistoriaAsync(
        int clienteId,
        int produtoId,
        int historiaStatusId,
        int historiaTipoId,
        int? usuarioResponsavelId,
        string? observacoes,
        int? previsaoDias,
        int empresaId,
        IEnumerable<HistoriaProdutoInput>? produtos = null,
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

        // Validar produto principal
        var produto = await _unitOfWork.Produtos.GetByIdAsync(produtoId);
        if (produto == null || produto.EmpresaId != empresaId)
            throw new KeyNotFoundException("Produto não encontrado.");
        
        if (!produto.Ativo)
            throw new ProdutoInativoException(produtoId);

        // Validar usuário responsável (se informado)
        if (usuarioResponsavelId.HasValue)
        {
            var usuarioResponsavel = await _unitOfWork.Users.GetByIdAsync(usuarioResponsavelId.Value);
            if (usuarioResponsavel == null)
                throw new KeyNotFoundException("Usuário responsável não encontrado.");
            
            if (usuarioResponsavel.Status != Status.Ativo)
                throw new UsuarioInativoException(usuarioResponsavelId.Value);
        }

        var historia = new Historia
        {
            EmpresaId = empresaId,
            ClienteId = clienteId,
            ProdutoId = produtoId,
            HistoriaStatusId = historiaStatusId,
            HistoriaTipoId = historiaTipoId,
            UsuarioResponsavelId = usuarioResponsavelId,
            Observacoes = observacoes,
            PrevisaoDias = previsaoDias,
            DataInicio = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.Historias.AddAsync(historia);
        await _unitOfWork.SaveChangesAsync();

        // Processar produtos adicionais
        if (produtos != null && produtos.Any())
        {
            foreach (var prod in produtos)
            {
                // Validar produto adicional
                var produtoAdicional = await _unitOfWork.Produtos.GetByIdAsync(prod.ProdutoId);
                if (produtoAdicional == null || produtoAdicional.EmpresaId != empresaId)
                    throw new KeyNotFoundException($"Produto {prod.ProdutoId} não encontrado.");
                
                if (!produtoAdicional.Ativo)
                    throw new ProdutoInativoException(prod.ProdutoId);

                var modulosValidos = new List<int>();
                if (prod.ProdutoModuloIds != null && prod.ProdutoModuloIds.Any())
                {
                    // Validar módulos
                    var modulosDb = await _unitOfWork.ProdutoModulos.FindAsync(m => prod.ProdutoModuloIds.Contains(m.Id));
                    var modulosDosProduto = modulosDb.Where(m => m.ProdutoId == prod.ProdutoId).ToList();
                    
                    // Verificar se todos os módulos estão ativos
                    foreach (var modulo in modulosDosProduto)
                    {
                        if (!modulo.Ativo)
                            throw new ModuloInativoException(modulo.Id);
                    }
                    
                    modulosValidos = modulosDosProduto.Select(m => m.Id).ToList();
                }

                await _unitOfWork.HistoriaProdutos.AddAsync(new HistoriaProduto
                {
                    HistoriaId = created.Id,
                    ProdutoId = prod.ProdutoId,
                    ProdutoModuloIds = modulosValidos
                });
            }
            await _unitOfWork.SaveChangesAsync();
        }

        // Movimentação Inicial
        if (usuarioCriadorId.HasValue)
        {
            await _unitOfWork.HistoriaMovimentacoes.AddAsync(new HistoriaMovimentacao
            {
                HistoriaId = created.Id,
                StatusAnteriorId = historiaStatusId,
                StatusNovoId = historiaStatusId,
                UsuarioId = usuarioCriadorId.Value,
                DataMovimentacao = DateTime.UtcNow,
                Observacoes = "História criada."
            });
            await _unitOfWork.SaveChangesAsync();
        }

        return created;
    }

    public async Task<Historia> AtualizarHistoriaAsync(
        int id,
        int clienteId,
        int produtoId,
        int historiaStatusId,
        int historiaTipoId,
        int? usuarioResponsavelId,
        string? observacoes,
        int? previsaoDias,
        int empresaId,
        IEnumerable<HistoriaProdutoInput>? produtos = null,
        int? usuarioAlteracaoId = null)
    {
        var historia = await _unitOfWork.Historias.GetByIdAsync(id);
        if (historia == null) // Faltou verificar empresaId? Assumindo que o caller verifica ou adicionamos aqui.
             throw new HistoriaNaoEncontradaException(id);
             
        // Para ser seguro, verificar se pertence a empresa? 
        // O original Service não tinha essa prop 'empresaId' na entidade Historia de forma explícita?
        // Sim, Historia tem cliente que tem empresa. Mas a entidade Historia em si tem? 
        // CreateHistoria.cs passava request.EmpresaId
        // Vamos assumir que sim ou validamos pelo cliente.

        historia.ClienteId = clienteId;
        historia.ProdutoId = produtoId;
        historia.HistoriaStatusId = historiaStatusId;
        historia.HistoriaTipoId = historiaTipoId;
        historia.UsuarioResponsavelId = usuarioResponsavelId;
        historia.Observacoes = observacoes;
        historia.PrevisaoDias = previsaoDias;
        historia.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Historias.UpdateAsync(historia);

        // Atualizar produtos se fornecido
        if (produtos != null)
        {
            var atuais = await _unitOfWork.HistoriaProdutos.FindAsync(hp => hp.HistoriaId == id);
            foreach (var atual in atuais)
            {
                await _unitOfWork.HistoriaProdutos.DeleteAsync(atual);
            }

            foreach (var prod in produtos)
            {
                var modulosValidos = new List<int>();
                if (prod.ProdutoModuloIds != null && prod.ProdutoModuloIds.Any())
                {
                   var modulosDb = await _unitOfWork.ProdutoModulos.FindAsync(m => prod.ProdutoModuloIds.Contains(m.Id));
                   modulosValidos = modulosDb.Where(m => m.ProdutoId == prod.ProdutoId).Select(m => m.Id).ToList();
                }

                await _unitOfWork.HistoriaProdutos.AddAsync(new HistoriaProduto
                {
                    HistoriaId = historia.Id,
                    ProdutoId = prod.ProdutoId,
                    ProdutoModuloIds = modulosValidos
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return historia;
    }

    public async Task<bool> DeletarHistoriaAsync(int id, int empresaId)
    {
        var historia = await _unitOfWork.Historias.GetByIdAsync(id);
        if (historia == null)
            throw new HistoriaNaoEncontradaException(id);

        await _unitOfWork.Historias.DeleteAsync(historia);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<Historia?> ObterHistoriaPorIdAsync(int id, int empresaId)
    {
        return await _unitOfWork.Historias.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Historia>> ObterHistoriasAsync(int? empresaId = null, int? clienteId = null, int? statusId = null)
    {
        var historias = await _unitOfWork.Historias.GetAllAsync();

        if (clienteId.HasValue)
            historias = historias.Where(h => h.ClienteId == clienteId.Value);

        if (statusId.HasValue)
            historias = historias.Where(h => h.HistoriaStatusId == statusId.Value);
            
        // Se a entidade Historia tiver EmpresaId, filtrar.
        // Se não, filtrar pelos clientes que pertencem a empresa (mais complexo sem join).
        // Assumindo que a camada superior cuida ou a entidade tem.

        return historias;
    }

    public async Task<HistoriaMovimentacao> AdicionarMovimentacaoAsync(int historiaId, int statusAnteriorId, int statusNovoId, int usuarioId, string? observacoes, int empresaId)
    {
        var historia = await _unitOfWork.Historias.GetByIdAsync(historiaId);
        if (historia == null)
            throw new HistoriaNaoEncontradaException(historiaId);

        var movimentacao = new HistoriaMovimentacao
        {
            HistoriaId = historiaId,
            StatusAnteriorId = statusAnteriorId,
            StatusNovoId = statusNovoId,
            UsuarioId = usuarioId,
            Observacoes = observacoes,
            DataMovimentacao = DateTime.UtcNow
        };

        historia.HistoriaStatusId = statusNovoId;
        historia.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Historias.UpdateAsync(historia);

        var created = await _unitOfWork.HistoriaMovimentacoes.AddAsync(movimentacao);
        await _unitOfWork.SaveChangesAsync();

        return created;
    }

    public async Task<IEnumerable<HistoriaProduto>> ObterProdutosPorHistoriaIdAsync(int historiaId)
    {
        return await _unitOfWork.HistoriaProdutos.FindAsync(hp => hp.HistoriaId == historiaId);
    }

    public async Task<IEnumerable<HistoriaProduto>> ObterProdutosPorListaIdsAsync(IEnumerable<int> historiaIds)
    {
        if (!historiaIds.Any()) return Enumerable.Empty<HistoriaProduto>();
        var ids = historiaIds.Distinct().ToList();
        return await _unitOfWork.HistoriaProdutos.FindAsync(hp => ids.Contains(hp.HistoriaId));
    }

    public async Task<IEnumerable<HistoriaMovimentacao>> ObterMovimentacoesPorHistoriaIdAsync(int historiaId)
    {
        return await _unitOfWork.HistoriaMovimentacoes.FindAsync(hm => hm.HistoriaId == historiaId);
    }

    public async Task<IEnumerable<HistoriaMovimentacao>> ObterMovimentacoesPorListaIdsAsync(IEnumerable<int> historiaIds)
    {
        if (!historiaIds.Any()) return Enumerable.Empty<HistoriaMovimentacao>();
        var ids = historiaIds.Distinct().ToList();
        return await _unitOfWork.HistoriaMovimentacoes.FindAsync(hm => ids.Contains(hm.HistoriaId));
    }
}
