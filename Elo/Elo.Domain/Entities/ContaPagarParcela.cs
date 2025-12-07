using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class ContaPagarParcela
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ContaPagarId { get; set; }
    public int Numero { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public ContaStatus Status { get; set; } = ContaStatus.Pendente;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ContaPagar ContaPagar { get; set; } = null!;
}
