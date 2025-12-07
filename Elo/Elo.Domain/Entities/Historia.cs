using System.Collections.Generic;

namespace Elo.Domain.Entities;

public class Historia
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ProdutoId { get; set; }
    public int HistoriaStatusId { get; set; }
    public int HistoriaTipoId { get; set; }
    public int? UsuarioResponsavelId { get; set; }
    public int? PrevisaoDias { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Pessoa Cliente { get; set; } = null!;
    public virtual Produto Produto { get; set; } = null!;
    public virtual HistoriaStatus Status { get; set; } = null!;
    public virtual HistoriaTipo Tipo { get; set; } = null!;
    public virtual User? UsuarioResponsavel { get; set; }
    public virtual ICollection<HistoriaProduto> Produtos { get; set; } = new List<HistoriaProduto>();
    public virtual ICollection<HistoriaMovimentacao> Movimentacoes { get; set; } = new List<HistoriaMovimentacao>();
}
