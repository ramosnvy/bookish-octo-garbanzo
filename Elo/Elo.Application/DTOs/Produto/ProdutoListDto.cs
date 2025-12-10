namespace Elo.Application.DTOs.Produto;

/// <summary>
/// DTO simplificado para listagem de produtos (sem módulos completos)
/// </summary>
public class ProdutoListDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal? Preco { get; set; }
    public Domain.Enums.Status Status { get; set; }
    public string StatusNome { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Informações resumidas
    public int QuantidadeModulos { get; set; }
    public bool PossuiModulos => QuantidadeModulos > 0;
}
