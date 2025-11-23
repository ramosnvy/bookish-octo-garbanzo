using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class Implantacao
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ProdutoId { get; set; }
    public ImplantacaoStatus Status { get; set; }
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
    public virtual ICollection<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();
}
