using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class ContaPagar
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int? FornecedorId { get; set; }
    public int? AfiliadoId { get; set; }
    public int? AssinaturaId { get; set; }
    public int? ContaReceberId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public ContaStatus Status { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool IsRecorrente { get; set; }
    public int TotalParcelas { get; set; } = 1;
    public int IntervaloDias { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Pessoa? Fornecedor { get; set; }
    public virtual Afiliado? Afiliado { get; set; }
    public virtual Empresa Empresa { get; set; } = null!;
    public virtual ICollection<ContaPagarItem> Itens { get; set; } = new List<ContaPagarItem>();
    public virtual ICollection<ContaPagarParcela> Parcelas { get; set; } = new List<ContaPagarParcela>();
}
