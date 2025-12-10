using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class EmpresaConfiguracao
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    
    // Juros
    public decimal JurosValor { get; set; }
    public TipoValor JurosTipo { get; set; }

    // Mora (Multa/Penalty)
    public decimal MoraValor { get; set; }
    public TipoValor MoraTipo { get; set; }

    // Afiliados
    /// <summary>
    /// Day of the month to generate payment for affiliates (e.g. 5, 10, 15)
    /// </summary>
    public int DiaPagamentoAfiliado { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
