using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class Movimentacao
{
    public int Id { get; set; }
    public int ImplantacaoId { get; set; }
    public ImplantacaoStatus StatusAnterior { get; set; }
    public ImplantacaoStatus StatusNovo { get; set; }
    public int UsuarioId { get; set; }
    public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow;
    public string? Observacoes { get; set; }

    // Navigation properties
    public virtual Implantacao Implantacao { get; set; } = null!;
    public virtual User Usuario { get; set; } = null!;
}
