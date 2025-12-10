using Elo.Domain.Entities;

namespace Elo.Domain.Entities;

public class AssinaturaItem
{
    public int Id { get; set; }
    public int AssinaturaId { get; set; }
    public int ProdutoId { get; set; }
    public int? ProdutoModuloId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Assinatura Assinatura { get; set; } = null!;
    public virtual Produto Produto { get; set; } = null!;
    public virtual ProdutoModulo? ProdutoModulo { get; set; }
}
