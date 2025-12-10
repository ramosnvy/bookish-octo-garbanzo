using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int TicketTipoId { get; set; }
    public TicketPrioridade Prioridade { get; set; }
    public TicketStatus Status { get; set; }
    public string NumeroExterno { get; set; } = string.Empty;
    public int? UsuarioAtribuidoId { get; set; }
    public int? ProdutoId { get; set; }
    public int? FornecedorId { get; set; }
    public int EmpresaId { get; set; }
    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
    public DateTime? DataFechamento { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Pessoa Cliente { get; set; } = null!;
    public virtual User? UsuarioAtribuido { get; set; }
    public virtual TicketTipo TicketTipo { get; set; } = null!;
    public virtual Produto? Produto { get; set; }
    public virtual Pessoa? Fornecedor { get; set; }
    public virtual Empresa Empresa { get; set; } = null!;
    public virtual ICollection<RespostaTicket> Respostas { get; set; } = new List<RespostaTicket>();
    public virtual ICollection<TicketAnexo> Anexos { get; set; } = new List<TicketAnexo>();
}
