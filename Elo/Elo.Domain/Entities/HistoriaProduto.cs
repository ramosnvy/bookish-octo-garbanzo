using System.Collections.Generic;

namespace Elo.Domain.Entities;

public class HistoriaProduto
{
    public int Id { get; set; }
    public int HistoriaId { get; set; }
    public Historia Historia { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public List<int> ProdutoModuloIds { get; set; } = new();
}
