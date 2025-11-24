using Elo.Domain.Enums;

namespace Elo.Application.DTOs.Ticket;

public class TicketDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TicketTipo Tipo { get; set; }
    public TicketPrioridade Prioridade { get; set; }
    public TicketStatus Status { get; set; }
    public int? UsuarioAtribuidoId { get; set; }
    public string? UsuarioAtribuidoNome { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<RespostaTicketDto> Respostas { get; set; } = Enumerable.Empty<RespostaTicketDto>();
}

public class CreateTicketDto
{
    public int ClienteId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TicketTipo Tipo { get; set; }
    public TicketPrioridade Prioridade { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Aberto;
    public int? UsuarioAtribuidoId { get; set; }
}

public class UpdateTicketDto : CreateTicketDto
{
    public int Id { get; set; }
    public DateTime? DataFechamento { get; set; }
}

public class RespostaTicketDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataResposta { get; set; }
    public bool IsInterna { get; set; }
}

public class CreateRespostaTicketDto
{
    public string Mensagem { get; set; } = string.Empty;
    public bool IsInterna { get; set; }
}
