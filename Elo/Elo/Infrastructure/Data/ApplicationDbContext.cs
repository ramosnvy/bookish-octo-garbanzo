using Microsoft.EntityFrameworkCore;
using Elo.Domain.Entities;

namespace Elo.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Implantacao> Implantacoes { get; set; }
    public DbSet<Movimentacao> Movimentacoes { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<RespostaTicket> RespostasTicket { get; set; }
    public DbSet<ContaReceber> ContasReceber { get; set; }
    public DbSet<ContaPagar> ContasPagar { get; set; }
    public DbSet<ContaPagarItem> ContaPagarItens { get; set; }
    public DbSet<ContaReceberItem> ContaReceberItens { get; set; }
    public DbSet<ContaPagarParcela> ContaPagarParcelas { get; set; }
    public DbSet<ContaReceberParcela> ContaReceberParcelas { get; set; }
    public DbSet<PessoaEndereco> PessoaEnderecos { get; set; }
    public DbSet<FornecedorCategoria> FornecedorCategorias { get; set; }
    public DbSet<ProdutoModulo> ProdutoModulos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Documento).HasMaxLength(50);
            entity.Property(e => e.EmailContato).HasMaxLength(150);
            entity.Property(e => e.TelefoneContato).HasMaxLength(30);
        });

        // Pessoa configuration
        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Documento).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Telefone).HasMaxLength(20);
            entity.Property(e => e.Categoria).HasMaxLength(150);
            entity.Property(e => e.Tipo).IsRequired();
            entity.HasIndex(e => new { e.EmpresaId, e.Documento }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.Email }).IsUnique();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FornecedorCategoria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(120);
            entity.HasIndex(e => new { e.EmpresaId, e.Nome }).IsUnique();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PessoaEndereco>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Logradouro).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Numero).HasMaxLength(50);
            entity.Property(e => e.Bairro).HasMaxLength(120);
            entity.Property(e => e.Cidade).HasMaxLength(120);
            entity.Property(e => e.Estado).HasMaxLength(60);
            entity.Property(e => e.Cep).HasMaxLength(30);
            entity.Property(e => e.Complemento).HasMaxLength(200);

            entity.HasOne(e => e.Pessoa)
                .WithMany(c => c.Enderecos)
                .HasForeignKey(e => e.PessoaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Pessoa>()
            .HasOne(p => p.FornecedorCategoria)
            .WithMany(c => c.Pessoas)
            .HasForeignKey(p => p.FornecedorCategoriaId)
            .OnDelete(DeleteBehavior.SetNull);

        // Produto configuration
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.ValorCusto).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ValorRevenda).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MargemLucro).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Fornecedor)
                .WithMany(p => p.ProdutosFornecidos)
                .HasForeignKey(e => e.FornecedorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Modulos)
                .WithOne(m => m.Produto)
                .HasForeignKey(m => m.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProdutoModulo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.ValorAdicional).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CustoAdicional).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Ativo).HasDefaultValue(true);
        });

        // Implantacao configuration
        modelBuilder.Entity<Implantacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Observacoes).HasMaxLength(1000);

            entity.HasOne(d => d.Cliente)
                .WithMany(p => p.Implantacoes)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Produto)
                .WithMany(p => p.Implantacoes)
                .HasForeignKey(d => d.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UsuarioResponsavel)
                .WithMany()
                .HasForeignKey(d => d.UsuarioResponsavelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Movimentacao configuration
        modelBuilder.Entity<Movimentacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Observacoes).HasMaxLength(500);

            entity.HasOne(d => d.Implantacao)
                .WithMany(p => p.Movimentacoes)
                .HasForeignKey(d => d.ImplantacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Ticket configuration
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(2000);

            entity.HasOne(d => d.Cliente)
                .WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UsuarioAtribuido)
                .WithMany()
                .HasForeignKey(d => d.UsuarioAtribuidoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // RespostaTicket configuration
        modelBuilder.Entity<RespostaTicket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Mensagem).IsRequired().HasMaxLength(2000);

            entity.HasOne(d => d.Ticket)
                .WithMany(p => p.Respostas)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ContaReceber configuration
        modelBuilder.Entity<ContaReceber>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Itens)
                .WithOne(i => i.ContaReceber)
                .HasForeignKey(i => i.ContaReceberId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Parcelas)
                .WithOne(p => p.ContaReceber)
                .HasForeignKey(p => p.ContaReceberId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Cliente)
                .WithMany(p => p.ContasReceber)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ContaPagar configuration
        modelBuilder.Entity<ContaPagar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Categoria).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Itens)
                .WithOne(i => i.ContaPagar)
                .HasForeignKey(i => i.ContaPagarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Parcelas)
                .WithOne(p => p.ContaPagar)
                .HasForeignKey(p => p.ContaPagarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Fornecedor)
                .WithMany(p => p.ContasPagar)
                .HasForeignKey(d => d.FornecedorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContaPagarItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descricao).HasMaxLength(300);
            entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ProdutoModuloIds)
                .HasColumnType("integer[]")
                .HasDefaultValueSql("'{}'::integer[]");
        });

        modelBuilder.Entity<ContaReceberItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descricao).HasMaxLength(300);
            entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ProdutoModuloIds)
                .HasColumnType("integer[]")
                .HasDefaultValueSql("'{}'::integer[]");
        });

        modelBuilder.Entity<ContaPagarParcela>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<ContaReceberParcela>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
        });
    }
}
