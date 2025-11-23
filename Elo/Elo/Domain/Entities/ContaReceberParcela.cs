using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class ContaReceberParcela
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ContaReceberId { get; set; }
    public int Numero { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataRecebimento { get; set; }
    public ContaStatus Status { get; set; } = ContaStatus.Pendente;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ContaReceber ContaReceber { get; set; } = null!;
}
