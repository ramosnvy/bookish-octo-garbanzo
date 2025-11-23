namespace Elo.Domain.Models;

public class ProdutoModuloInput
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorAdicional { get; set; }
    public decimal CustoAdicional { get; set; }
    public bool Ativo { get; set; } = true;
}
