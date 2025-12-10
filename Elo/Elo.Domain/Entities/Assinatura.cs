using Elo.Domain.Entities;
using Elo.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Elo.Domain.Entities;

public class Assinatura
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ClienteId { get; set; }
    public int? AfiliadoId { get; set; }
    
    /// <summary>
    /// Indicates if the subscription is actively running.
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// If true, the subscription will auto-renew or generate recurring billing.
    /// </summary>
    public bool IsRecorrente { get; set; }

    /// <summary>
    /// Interval in days for recurrence (e.g., 30 for monthly).
    /// </summary>
    public int? IntervaloDias { get; set; }

    /// <summary>
    /// Number of recurrences to generate (e.g., 12 for yearly subscription paid monthly).
    /// </summary>
    public int? RecorrenciaQtde { get; set; }

    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    /// <summary>
    /// Payment method for this subscription.
    /// </summary>
    public FormaPagamento? FormaPagamento { get; set; }

    /// <summary>
    /// Configuration: Should this subscription generate Accounts Payable/Receivable?
    /// </summary>
    public bool GerarFinanceiro { get; set; }

    /// <summary>
    /// Configuration: Should this subscription generate a Kanban Implementation card?
    /// </summary>
    public bool GerarImplantacao { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual Pessoa Cliente { get; set; } = null!;
    public virtual Empresa? Empresa { get; set; }
    public virtual Afiliado? Afiliado { get; set; }
    public virtual ICollection<AssinaturaItem> Itens { get; set; } = new List<AssinaturaItem>();
}
