namespace Elo.Domain.Entities;

public class ProdutoModulo
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorAdicional { get; set; }
    public decimal CustoAdicional { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Produto Produto { get; set; } = null!;
}
