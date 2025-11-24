using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class Produto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorCusto { get; set; }
    public decimal ValorRevenda { get; set; }
    public decimal MargemLucro { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? FornecedorId { get; set; }
    public virtual Pessoa? Fornecedor { get; set; }
    public virtual Empresa Empresa { get; set; } = null!;

    // Navigation properties
    public virtual ICollection<Historia> Historias { get; set; } = new List<Historia>();
    public virtual ICollection<ProdutoModulo> Modulos { get; set; } = new List<ProdutoModulo>();
}
