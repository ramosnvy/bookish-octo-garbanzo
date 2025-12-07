using System.Collections.Generic;

namespace Elo.Domain.Entities;

public class ContaReceberItem
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ContaReceberId { get; set; }
    public int? ProdutoId { get; set; }
    public List<int> ProdutoModuloIds { get; set; } = new();
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ContaReceber ContaReceber { get; set; } = null!;
}
