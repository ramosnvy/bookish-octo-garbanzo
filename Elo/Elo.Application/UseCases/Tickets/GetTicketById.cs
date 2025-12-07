using System.Collections.Generic;
using System.Linq;
using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Tickets;

public static class GetTicketById
{
    public class Query : IRequest<TicketDto?>
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, TicketDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TicketDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(request.Id);
            if (ticket == null)
            {
                return null;
            }

            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente)
            {
                return null;
            }

            if (request.EmpresaId.HasValue && cliente.EmpresaId != request.EmpresaId.Value)
            {
                return null;
            }

            var respostas = await _unitOfWork.RespostasTicket.FindAsync(r => r.TicketId == ticket.Id);
            var anexos = await _unitOfWork.TicketAnexos.FindAsync(a => a.TicketId == ticket.Id);

            var usuarioIds = respostas.Select(r => r.UsuarioId)
                .Concat(ticket.UsuarioAtribuidoId.HasValue
                    ? new[] { ticket.UsuarioAtribuidoId.Value }
                    : Enumerable.Empty<int>())
                .Concat(anexos.Select(a => a.UsuarioId))
                .Distinct()
                .ToList();

            var usuarios = usuarioIds.Any()
                ? await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id))
                : Enumerable.Empty<User>();

            var ticketTipo = await _unitOfWork.TicketTipos.GetByIdAsync(ticket.TicketTipoId);
            var ticketTipoLookup = ticketTipo != null
                ? new Dictionary<int, TicketTipo> { { ticketTipo.Id, ticketTipo } }
                : new Dictionary<int, TicketTipo>();

            Produto? produto = null;
            if (ticket.ProdutoId.HasValue)
            {
                produto = await _unitOfWork.Produtos.GetByIdAsync(ticket.ProdutoId.Value);
            }

            Pessoa? fornecedor = null;
            if (ticket.FornecedorId.HasValue)
            {
                fornecedor = await _unitOfWork.Pessoas.GetByIdAsync(ticket.FornecedorId.Value);
            }

            var clienteLookup = new Dictionary<int, Pessoa> { { cliente.Id, cliente } };
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);
            var respostasLookup = new Dictionary<int, List<RespostaTicket>>
            {
                { ticket.Id, respostas.ToList() }
            };
            var anexosLookup = new Dictionary<int, List<TicketAnexo>>
            {
                { ticket.Id, anexos.ToList() }
            };
            var produtoLookup = produto != null
                ? new Dictionary<int, Produto> { { produto.Id, produto } }
                : new Dictionary<int, Produto>();
            var fornecedorLookup = fornecedor != null
                ? new Dictionary<int, Pessoa> { { fornecedor.Id, fornecedor } }
                : new Dictionary<int, Pessoa>();

            return TicketMapper.ToDto(
                ticket,
                clienteLookup,
                usuarioLookup,
                respostasLookup,
                ticketTipoLookup,
                produtoLookup,
                fornecedorLookup,
                anexosLookup);
        }
    }
}
