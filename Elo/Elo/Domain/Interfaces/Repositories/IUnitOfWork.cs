using Elo.Domain.Entities;

namespace Elo.Domain.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Empresa> Empresas { get; }
    IRepository<User> Users { get; }
    IRepository<Pessoa> Pessoas { get; }
    IRepository<Produto> Produtos { get; }
    IRepository<ProdutoModulo> ProdutoModulos { get; }
    IRepository<Implantacao> Implantacoes { get; }
    IRepository<Movimentacao> Movimentacoes { get; }
    IRepository<Ticket> Tickets { get; }
    IRepository<RespostaTicket> RespostasTicket { get; }
    IRepository<ContaReceber> ContasReceber { get; }
    IRepository<ContaPagar> ContasPagar { get; }
    IRepository<ContaPagarItem> ContaPagarItens { get; }
    IRepository<ContaReceberItem> ContaReceberItens { get; }
    IRepository<ContaPagarParcela> ContaPagarParcelas { get; }
    IRepository<ContaReceberParcela> ContaReceberParcelas { get; }
    IRepository<PessoaEndereco> PessoaEnderecos { get; }
    IRepository<FornecedorCategoria> FornecedorCategorias { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
