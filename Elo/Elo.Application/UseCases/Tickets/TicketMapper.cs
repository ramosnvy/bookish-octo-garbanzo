using System.Collections.Generic;
using System.Linq;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;

namespace Elo.Application.UseCases.Tickets;

internal static class TicketMapper
{
    public static TicketDto ToDto(
        Ticket ticket,
        IReadOnlyDictionary<int, Pessoa> clientes,
        IReadOnlyDictionary<int, User> usuarios,
        IReadOnlyDictionary<int, List<RespostaTicket>> respostasLookup,
        IReadOnlyDictionary<int, TicketTipo> ticketTipos,
        IReadOnlyDictionary<int, Produto> produtos,
        IReadOnlyDictionary<int, Pessoa> fornecedores,
        IReadOnlyDictionary<int, List<TicketAnexo>> anexosLookup)
    {
        clientes.TryGetValue(ticket.ClienteId, out var cliente);
        ticketTipos.TryGetValue(ticket.TicketTipoId, out var ticketTipo);

        Produto? produto = null;
        if (ticket.ProdutoId.HasValue)
        {
            produtos.TryGetValue(ticket.ProdutoId.Value, out produto);
        }

        Pessoa? fornecedor = null;
        if (ticket.FornecedorId.HasValue)
        {
            fornecedores.TryGetValue(ticket.FornecedorId.Value, out fornecedor);
        }

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

        var anexos = anexosLookup.TryGetValue(ticket.Id, out var anexosTicket)
            ? anexosTicket
                .OrderBy(a => a.CreatedAt)
                .Select(a => new TicketAnexoDto
                {
                    Id = a.Id,
                    TicketId = a.TicketId,
                    Nome = a.Nome,
                    MimeType = a.MimeType,
                    Tamanho = a.Tamanho,
                    CreatedAt = a.CreatedAt
                })
            : Enumerable.Empty<TicketAnexoDto>();

        return new TicketDto
        {
            Id = ticket.Id,
            ClienteId = ticket.ClienteId,
            ClienteNome = cliente?.Nome ?? string.Empty,
            TicketTipoId = ticket.TicketTipoId,
            TicketTipoNome = ticketTipo?.Nome ?? string.Empty,
            Titulo = ticket.Titulo,
            Descricao = ticket.Descricao,
            Prioridade = ticket.Prioridade,
            Status = ticket.Status,
            ProdutoId = ticket.ProdutoId,
            ProdutoNome = produto?.Nome,
            FornecedorId = ticket.FornecedorId,
            FornecedorNome = fornecedor?.Nome,
            NumeroExterno = ticket.NumeroExterno,
            UsuarioAtribuidoId = ticket.UsuarioAtribuidoId,
            UsuarioAtribuidoNome = usuarioAtribuido?.Nome,
            DataAbertura = ticket.DataAbertura,
            DataFechamento = ticket.DataFechamento,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            Respostas = respostas,
            Anexos = anexos
        };
    }
}
