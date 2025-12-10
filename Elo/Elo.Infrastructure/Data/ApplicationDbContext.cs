using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<Historia> Historias { get; set; }
    public DbSet<HistoriaProduto> HistoriaProdutos { get; set; }
    public DbSet<HistoriaMovimentacao> HistoriaMovimentacoes { get; set; }
    public DbSet<HistoriaStatus> HistoriaStatuses { get; set; }
    public DbSet<HistoriaTipo> HistoriaTipos { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketTipo> TicketTipos { get; set; }
    public DbSet<TicketAnexo> TicketAnexos { get; set; }
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
    public DbSet<Afiliado> Afiliados { get; set; }
    public DbSet<Assinatura> Assinaturas { get; set; }
    public DbSet<AssinaturaItem> AssinaturaItens { get; set; }
    public DbSet<EmpresaFormaPagamento> EmpresaFormasPagamento { get; set; }
    public DbSet<EmpresaConfiguracao> EmpresaConfiguracoes { get; set; }

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
            entity.Property(e => e.Status).IsRequired().HasDefaultValue(Status.Ativo);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.Empresa)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RazaoSocial).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NomeFantasia).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Cnpj).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Ie).HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Telefone).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Endereco).IsRequired().HasMaxLength(500);
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
            entity.Property(e => e.ServicoPagamentoTipo).IsRequired().HasDefaultValue(ServicoPagamentoTipo.PrePago);
            entity.Property(e => e.PrazoPagamentoDias).IsRequired().HasDefaultValue(0);

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

        // Historia configuration
        modelBuilder.Entity<Historia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Observacoes).HasMaxLength(1000);
            entity.Property(e => e.PrevisaoDias);

            entity.HasOne(d => d.Empresa)
                .WithMany()
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Cliente)
                .WithMany(p => p.Historias)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Produto)
                .WithMany(p => p.Historias)
                .HasForeignKey(d => d.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Status)
                .WithMany(s => s.Historias)
                .HasForeignKey(d => d.HistoriaStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Tipo)
                .WithMany(t => t.Historias)
                .HasForeignKey(d => d.HistoriaTipoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UsuarioResponsavel)
                .WithMany()
                .HasForeignKey(d => d.UsuarioResponsavelId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Produtos)
                .WithOne(p => p.Historia)
                .HasForeignKey(p => p.HistoriaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HistoriaProduto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProdutoModuloIds)
                .HasColumnType("integer[]")
                .HasDefaultValueSql("'{}'::integer[]");

            entity.HasOne(e => e.Historia)
                .WithMany(h => h.Produtos)
                .HasForeignKey(e => e.HistoriaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Produto)
                .WithMany(p => p.HistoriaProdutos)
                .HasForeignKey(e => e.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // HistoriaMovimentacao configuration
        modelBuilder.Entity<HistoriaMovimentacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Observacoes).HasMaxLength(500);

            entity.HasOne(d => d.Historia)
                .WithMany(p => p.Movimentacoes)
                .HasForeignKey(d => d.HistoriaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.StatusAnterior)
                .WithMany(s => s.MovimentacoesComoAnterior)
                .HasForeignKey(d => d.StatusAnteriorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.StatusNovo)
                .WithMany(s => s.MovimentacoesComoNovo)
                .HasForeignKey(d => d.StatusNovoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HistoriaTipo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.Ordem).HasDefaultValue(0);
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.HasIndex(e => new { e.EmpresaId, e.Nome }).IsUnique();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HistoriaStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.Cor).HasMaxLength(20);
            entity.Property(e => e.Ordem).HasDefaultValue(0);
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.HasIndex(e => new { e.EmpresaId, e.Nome }).IsUnique();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Ticket configuration
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.NumeroExterno).HasMaxLength(60).HasDefaultValue(string.Empty);

            entity.HasOne(d => d.Empresa)
                .WithMany()
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Cliente)
                .WithMany(p => p.TicketsComoCliente)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.TicketTipo)
                .WithMany(t => t.Tickets)
                .HasForeignKey(d => d.TicketTipoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Produto)
                .WithMany()
                .HasForeignKey(d => d.ProdutoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Fornecedor)
                .WithMany(p => p.TicketsComoFornecedor)
                .HasForeignKey(d => d.FornecedorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.UsuarioAtribuido)
                .WithMany()
                .HasForeignKey(d => d.UsuarioAtribuidoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Anexos)
                .WithOne(a => a.Ticket)
                .HasForeignKey(a => a.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TicketTipo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.Ordem).HasDefaultValue(0);
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.HasIndex(e => new { e.EmpresaId, e.Nome }).IsUnique();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketAnexo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Conteudo).IsRequired();
            entity.Property(e => e.Tamanho).IsRequired();

            entity.HasOne(d => d.Ticket)
                .WithMany(t => t.Anexos)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RespostaTicket configuration
        modelBuilder.Entity<RespostaTicket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Mensagem).IsRequired().HasMaxLength(2000);

            entity.HasOne(d => d.Empresa)
                .WithMany()
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

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
            entity.HasIndex(e => new { e.EmpresaId, e.ContaReceberId });

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
            
            entity.HasOne(d => d.Afiliado)
                .WithMany()
                .HasForeignKey(d => d.AfiliadoId)
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

        // Afiliado configuration
        modelBuilder.Entity<Afiliado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Documento).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Telefone).HasMaxLength(20);
            entity.Property(e => e.Porcentagem).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => new { e.EmpresaId, e.Email }).IsUnique();
            entity.HasIndex(e => new { e.EmpresaId, e.Documento }).IsUnique();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Assinatura configuration
        modelBuilder.Entity<Assinatura>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.IsRecorrente).HasDefaultValue(false);

            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Afiliado)
                .WithMany()
                .HasForeignKey(e => e.AfiliadoId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Itens)
                .WithOne(i => i.Assinatura)
                .HasForeignKey(i => i.AssinaturaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AssinaturaItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Produto)
                .WithMany()
                .HasForeignKey(e => e.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProdutoModulo)
                .WithMany()
                .HasForeignKey(e => e.ProdutoModuloId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmpresaFormaPagamento configuration
        modelBuilder.Entity<EmpresaFormaPagamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FormaPagamento).IsRequired();
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.HasIndex(e => new { e.EmpresaId, e.FormaPagamento }).IsUnique();

            entity.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmpresaConfiguracao configuration
        modelBuilder.Entity<EmpresaConfiguracao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.JurosValor).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MoraValor).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiaPagamentoAfiliado).HasDefaultValue(1);

            entity.HasOne(e => e.Empresa)
                .WithOne(e => e.Configuracao)
                .HasForeignKey<EmpresaConfiguracao>(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
