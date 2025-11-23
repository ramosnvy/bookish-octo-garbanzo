namespace Elo.Domain.Entities;

public class FornecedorCategoria
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
    public virtual ICollection<Pessoa> Pessoas { get; set; } = new List<Pessoa>();
}
