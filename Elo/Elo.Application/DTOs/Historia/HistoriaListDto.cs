namespace Elo.Application.DTOs.Historia;

/// <summary>
/// DTO simplificado para listagem de histórias (sem relacionamentos complexos)
/// </summary>
public class HistoriaListDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int HistoriaStatusId { get; set; }
    public string HistoriaStatusNome { get; set; } = string.Empty;
    public string? HistoriaStatusCor { get; set; }
    public int HistoriaTipoId { get; set; }
    public string HistoriaTipoNome { get; set; } = string.Empty;
    public int? UsuarioResponsavelId { get; set; }
    public string UsuarioResponsavelNome { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public int? PrevisaoDias { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Propriedades computadas úteis para listagem
    public bool Atrasada => DataFim.HasValue && !DataFinalizacao.HasValue && DataFim < DateTime.Now;
    public int? DiasRestantes => DataFim.HasValue && !DataFinalizacao.HasValue 
        ? (int)(DataFim.Value - DateTime.Now).TotalDays 
        : null;
}
