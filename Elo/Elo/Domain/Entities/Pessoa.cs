using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class Pessoa
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int? FornecedorCategoriaId { get; set; }
    public Status Status { get; set; }
    public PessoaTipo Tipo { get; set; }
    public ServicoPagamentoTipo ServicoPagamentoTipo { get; set; } = ServicoPagamentoTipo.PrePago;
    public int PrazoPagamentoDias { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual FornecedorCategoria? FornecedorCategoria { get; set; }
    public virtual Empresa Empresa { get; set; } = null!;
    public virtual ICollection<PessoaEndereco> Enderecos { get; set; } = new List<PessoaEndereco>();
    public virtual ICollection<Historia> Historias { get; set; } = new List<Historia>();
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public virtual ICollection<ContaReceber> ContasReceber { get; set; } = new List<ContaReceber>();
    public virtual ICollection<ContaPagar> ContasPagar { get; set; } = new List<ContaPagar>();
    public virtual ICollection<Produto> ProdutosFornecidos { get; set; } = new List<Produto>();
}
