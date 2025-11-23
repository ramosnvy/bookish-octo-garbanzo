using System.Collections.Generic;

namespace Elo.Domain.Entities;

public class ContaPagarItem
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ContaPagarId { get; set; }
    public int? ProdutoId { get; set; }
    public List<int> ProdutoModuloIds { get; set; } = new();
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ContaPagar ContaPagar { get; set; } = null!;
}
