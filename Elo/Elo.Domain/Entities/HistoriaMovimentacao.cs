namespace Elo.Domain.Entities;

public class HistoriaMovimentacao
{
    public int Id { get; set; }
    public int HistoriaId { get; set; }
    public int StatusAnteriorId { get; set; }
    public int StatusNovoId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow;
    public string? Observacoes { get; set; }

    // Navigation properties
    public virtual Historia Historia { get; set; } = null!;
    public virtual HistoriaStatus StatusAnterior { get; set; } = null!;
    public virtual HistoriaStatus StatusNovo { get; set; } = null!;
    public virtual User Usuario { get; set; } = null!;
}
