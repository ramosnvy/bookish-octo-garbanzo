using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

/// <summary>
/// Representa as formas de pagamento disponíveis para uma empresa.
/// </summary>
public class EmpresaFormaPagamento
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool AVista { get; set; } = false;
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Empresa Empresa { get; set; } = null!;
}
