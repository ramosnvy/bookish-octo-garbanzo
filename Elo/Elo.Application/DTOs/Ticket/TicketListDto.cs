namespace Elo.Application.DTOs.Ticket;

/// <summary>
/// DTO simplificado para listagem de tickets (sem respostas e anexos)
/// </summary>
public class TicketListDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Domain.Enums.TicketStatus Status { get; set; }
    public string StatusNome { get; set; } = string.Empty;
    public Domain.Enums.TicketPrioridade Prioridade { get; set; }
    public string PrioridadeNome { get; set; } = string.Empty;
    public int? TipoId { get; set; }
    public string? TipoNome { get; set; }
    public int? ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public int? ProdutoId { get; set; }
    public string? ProdutoNome { get; set; }
    public int? FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public int? UsuarioAtribuidoId { get; set; }
    public string? UsuarioAtribuidoNome { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Propriedades computadas úteis
    public int QuantidadeRespostas { get; set; }
    public int QuantidadeAnexos { get; set; }
    public bool Aberto => Status != Domain.Enums.TicketStatus.Fechado;
}
