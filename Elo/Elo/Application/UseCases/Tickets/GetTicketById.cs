using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;
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
            if (cliente == null || cliente.Tipo != Domain.Enums.PessoaTipo.Cliente)
            {
                return null;
            }

            if (request.EmpresaId.HasValue && cliente.EmpresaId != request.EmpresaId.Value)
            {
                return null;
            }

            var respostas = await _unitOfWork.RespostasTicket.FindAsync(r => r.TicketId == ticket.Id);
            var usuarioIds = respostas.Select(r => r.UsuarioId).ToList();
            if (ticket.UsuarioAtribuidoId.HasValue)
            {
                usuarioIds.Add(ticket.UsuarioAtribuidoId.Value);
            }

            var usuarios = usuarioIds.Any()
                ? await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id))
                : Enumerable.Empty<User>();

            var clienteLookup = new Dictionary<int, Pessoa> { { cliente.Id, cliente } };
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);
            var respostasLookup = new Dictionary<int, List<RespostaTicket>>
            {
                { ticket.Id, respostas.ToList() }
            };

            return TicketMapper.ToDto(ticket, clienteLookup, usuarioLookup, respostasLookup);
        }
    }
}
