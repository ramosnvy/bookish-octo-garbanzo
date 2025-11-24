using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class Historia
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ProdutoId { get; set; }
    public HistoriaStatus Status { get; set; }
    public HistoriaTipo Tipo { get; set; } = HistoriaTipo.Projeto;
    public int UsuarioResponsavelId { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Pessoa Cliente { get; set; } = null!;
    public virtual Produto Produto { get; set; } = null!;
    public virtual User UsuarioResponsavel { get; set; } = null!;
    public virtual ICollection<HistoriaMovimentacao> Movimentacoes { get; set; } = new List<HistoriaMovimentacao>();
}
