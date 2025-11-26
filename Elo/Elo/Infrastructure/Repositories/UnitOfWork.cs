using Microsoft.EntityFrameworkCore.Storage;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces.Repositories;
using Elo.Infrastructure.Data;

namespace Elo.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Empresas = new Repository<Empresa>(_context);
        Users = new Repository<User>(_context);
        Pessoas = new Repository<Pessoa>(_context);
        Produtos = new Repository<Produto>(_context);
        ProdutoModulos = new Repository<ProdutoModulo>(_context);
        Historias = new Repository<Historia>(_context);
        HistoriaMovimentacoes = new Repository<HistoriaMovimentacao>(_context);
        HistoriaProdutos = new Repository<HistoriaProduto>(_context);
        Tickets = new Repository<Ticket>(_context);
        RespostasTicket = new Repository<RespostaTicket>(_context);
        ContasReceber = new Repository<ContaReceber>(_context);
        ContasPagar = new Repository<ContaPagar>(_context);
        ContaPagarItens = new Repository<ContaPagarItem>(_context);
        ContaReceberItens = new Repository<ContaReceberItem>(_context);
        ContaPagarParcelas = new Repository<ContaPagarParcela>(_context);
        ContaReceberParcelas = new Repository<ContaReceberParcela>(_context);
        PessoaEnderecos = new Repository<PessoaEndereco>(_context);
        FornecedorCategorias = new Repository<FornecedorCategoria>(_context);
    }

    public IRepository<Empresa> Empresas { get; }
    public IRepository<User> Users { get; }
    public IRepository<Pessoa> Pessoas { get; }
    public IRepository<Produto> Produtos { get; }
    public IRepository<ProdutoModulo> ProdutoModulos { get; }
    public IRepository<Historia> Historias { get; }
    public IRepository<HistoriaMovimentacao> HistoriaMovimentacoes { get; }
    public IRepository<HistoriaProduto> HistoriaProdutos { get; }
    public IRepository<Ticket> Tickets { get; }
    public IRepository<RespostaTicket> RespostasTicket { get; }
    public IRepository<ContaReceber> ContasReceber { get; }
    public IRepository<ContaPagar> ContasPagar { get; }
    public IRepository<ContaPagarItem> ContaPagarItens { get; }
    public IRepository<ContaReceberItem> ContaReceberItens { get; }
    public IRepository<ContaPagarParcela> ContaPagarParcelas { get; }
    public IRepository<ContaReceberParcela> ContaReceberParcelas { get; }
    public IRepository<PessoaEndereco> PessoaEnderecos { get; }
    public IRepository<FornecedorCategoria> FornecedorCategorias { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
