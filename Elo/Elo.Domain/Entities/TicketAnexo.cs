using System;

namespace Elo.Domain.Entities;

public class TicketAnexo
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Tamanho { get; set; }
    public byte[] Conteudo { get; set; } = Array.Empty<byte>();
    public int UsuarioId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Ticket Ticket { get; set; } = null!;
    public virtual User Usuario { get; set; } = null!;
}
