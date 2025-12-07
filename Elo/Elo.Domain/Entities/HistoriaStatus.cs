using System.Collections.Generic;

namespace Elo.Domain.Entities;

public class HistoriaStatus
{
    public int Id { get; set; }
    public int? EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Cor { get; set; }
    public bool FechaHistoria { get; set; } = false;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Empresa? Empresa { get; set; }
    public virtual ICollection<Historia> Historias { get; set; } = new List<Historia>();
    public virtual ICollection<HistoriaMovimentacao> MovimentacoesComoAnterior { get; set; } = new List<HistoriaMovimentacao>();
    public virtual ICollection<HistoriaMovimentacao> MovimentacoesComoNovo { get; set; } = new List<HistoriaMovimentacao>();
}
