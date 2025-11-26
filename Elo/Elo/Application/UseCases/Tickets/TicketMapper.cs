using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;

namespace Elo.Application.UseCases.Tickets;

internal static class TicketMapper
{
    public static TicketDto ToDto(
        Ticket ticket,
        IReadOnlyDictionary<int, Pessoa> clientes,
        IReadOnlyDictionary<int, User> usuarios,
        IReadOnlyDictionary<int, List<RespostaTicket>> respostasLookup)
    {
        clientes.TryGetValue(ticket.ClienteId, out var cliente);
        User? usuarioAtribuido = null;
        if (ticket.UsuarioAtribuidoId.HasValue)
        {
            usuarios.TryGetValue(ticket.UsuarioAtribuidoId.Value, out usuarioAtribuido);
        }

        var respostas = respostasLookup.TryGetValue(ticket.Id, out var respostasTicket)
            ? respostasTicket
                .OrderBy(r => r.DataResposta)
                .Select(r =>
                {
                    usuarios.TryGetValue(r.UsuarioId, out var usuarioResposta);
                    return new RespostaTicketDto
                    {
                        Id = r.Id,
                        TicketId = r.TicketId,
                        UsuarioId = r.UsuarioId,
                        UsuarioNome = usuarioResposta?.Nome ?? string.Empty,
                        Mensagem = r.Mensagem,
                        DataResposta = r.DataResposta,
                        IsInterna = r.IsInterna
                    };
                })
            : Enumerable.Empty<RespostaTicketDto>();

        return new TicketDto
        {
            Id = ticket.Id,
            ClienteId = ticket.ClienteId,
            ClienteNome = cliente?.Nome ?? string.Empty,
            Titulo = ticket.Titulo,
            Descricao = ticket.Descricao,
            Tipo = ticket.Tipo,
            Prioridade = ticket.Prioridade,
            Status = ticket.Status,
            NumeroExterno = ticket.NumeroExterno,
            UsuarioAtribuidoId = ticket.UsuarioAtribuidoId,
            UsuarioAtribuidoNome = usuarioAtribuido?.Nome,
            DataAbertura = ticket.DataAbertura,
            DataFechamento = ticket.DataFechamento,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            Respostas = respostas
        };
    }
}
