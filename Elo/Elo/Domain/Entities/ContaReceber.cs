using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class ContaReceber
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ClienteId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataRecebimento { get; set; }
    public ContaStatus Status { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public bool IsRecorrente { get; set; }
    public int TotalParcelas { get; set; } = 1;
    public int IntervaloDias { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Pessoa Cliente { get; set; } = null!;
    public virtual Empresa Empresa { get; set; } = null!;
    public virtual ICollection<ContaReceberItem> Itens { get; set; } = new List<ContaReceberItem>();
    public virtual ICollection<ContaReceberParcela> Parcelas { get; set; } = new List<ContaReceberParcela>();
}
