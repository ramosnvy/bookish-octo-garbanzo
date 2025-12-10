namespace Elo.Domain.Entities;

public class RespostaTicket
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int UsuarioId { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataResposta { get; set; } = DateTime.UtcNow;
    public bool IsInterna { get; set; } = false;
    public int EmpresaId { get; set; }

    // Navigation properties
    public virtual Ticket Ticket { get; set; } = null!;
    public virtual User Usuario { get; set; } = null!;
    public virtual Empresa Empresa { get; set; } = null!;
}
