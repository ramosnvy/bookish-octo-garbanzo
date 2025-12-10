using Elo.Domain.Entities;

namespace Elo.Domain.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Empresa> Empresas { get; }
    IRepository<User> Users { get; }
    IRepository<Pessoa> Pessoas { get; }
    IRepository<Produto> Produtos { get; }
    IRepository<ProdutoModulo> ProdutoModulos { get; }
    IRepository<Historia> Historias { get; }
    IRepository<HistoriaProduto> HistoriaProdutos { get; }
    IRepository<HistoriaMovimentacao> HistoriaMovimentacoes { get; }
    IRepository<HistoriaStatus> HistoriaStatuses { get; }
    IRepository<HistoriaTipo> HistoriaTipos { get; }
    IRepository<Ticket> Tickets { get; }
    IRepository<TicketTipo> TicketTipos { get; }
    IRepository<TicketAnexo> TicketAnexos { get; }
    IRepository<RespostaTicket> RespostasTicket { get; }
    IRepository<ContaReceber> ContasReceber { get; }
    IRepository<ContaPagar> ContasPagar { get; }
    IRepository<ContaPagarItem> ContaPagarItens { get; }
    IRepository<ContaReceberItem> ContaReceberItens { get; }
    IRepository<ContaPagarParcela> ContaPagarParcelas { get; }
    IRepository<ContaReceberParcela> ContaReceberParcelas { get; }
    IRepository<PessoaEndereco> PessoaEnderecos { get; }
    IRepository<FornecedorCategoria> FornecedorCategorias { get; }
    IRepository<Afiliado> Afiliados { get; }
    IRepository<Assinatura> Assinaturas { get; }
    IRepository<AssinaturaItem> AssinaturaItens { get; }
    IRepository<EmpresaFormaPagamento> EmpresaFormasPagamento { get; }
    IRepository<EmpresaConfiguracao> EmpresaConfiguracoes { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
