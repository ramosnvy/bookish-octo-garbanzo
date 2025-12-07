using System.Collections.Generic;
using System.Linq;
using Elo.Domain.Enums;

namespace Elo.Application.DTOs.Ticket;

public class TicketDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int TicketTipoId { get; set; }
    public string TicketTipoNome { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TicketPrioridade Prioridade { get; set; }
    public TicketStatus Status { get; set; }
    public int? ProdutoId { get; set; }
    public string? ProdutoNome { get; set; }
    public int? FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public string NumeroExterno { get; set; } = string.Empty;
    public int? UsuarioAtribuidoId { get; set; }
    public string? UsuarioAtribuidoNome { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<RespostaTicketDto> Respostas { get; set; } = Enumerable.Empty<RespostaTicketDto>();
    public IEnumerable<TicketAnexoDto> Anexos { get; set; } = Enumerable.Empty<TicketAnexoDto>();
}

public class CreateTicketDto
{
    public int ClienteId { get; set; }
    public int TicketTipoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TicketPrioridade Prioridade { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Aberto;
    public int? UsuarioAtribuidoId { get; set; }
    public int? ProdutoId { get; set; }
    public int? FornecedorId { get; set; }
    public string NumeroExterno { get; set; } = string.Empty;
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

public class TicketAnexoDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Tamanho { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TicketTipoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTicketTipoDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}

public class UpdateTicketTipoDto : CreateTicketTipoDto
{
    public int Id { get; set; }
}
