using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Tickets;

public static class GetAllTickets
{
    public class Query : IRequest<IEnumerable<TicketDto>>
    {
        public int? EmpresaId { get; set; }
        public TicketStatus? Status { get; set; }
        public TicketTipo? Tipo { get; set; }
        public TicketPrioridade? Prioridade { get; set; }
        public int? ClienteId { get; set; }
        public int? UsuarioAtribuidoId { get; set; }
        public DateTime? DataAberturaInicio { get; set; }
        public DateTime? DataAberturaFim { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<TicketDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TicketDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var clientes = await _unitOfWork.Pessoas.FindAsync(p =>
                p.Tipo == PessoaTipo.Cliente &&
                (!request.EmpresaId.HasValue || p.EmpresaId == request.EmpresaId.Value));
            var clienteLookup = clientes.ToDictionary(c => c.Id, c => c);

            var tickets = (await _unitOfWork.Tickets.GetAllAsync())
                .Where(t => !request.EmpresaId.HasValue || clienteLookup.ContainsKey(t.ClienteId))
                .ToList();

            if (request.Status.HasValue)
            {
                tickets = tickets.Where(t => t.Status == request.Status.Value).ToList();
            }

            if (request.Tipo.HasValue)
            {
                tickets = tickets.Where(t => t.Tipo == request.Tipo.Value).ToList();
            }

            if (request.Prioridade.HasValue)
            {
                tickets = tickets.Where(t => t.Prioridade == request.Prioridade.Value).ToList();
            }

            if (request.ClienteId.HasValue)
            {
                tickets = tickets.Where(t => t.ClienteId == request.ClienteId.Value).ToList();
            }

            if (request.UsuarioAtribuidoId.HasValue)
            {
                tickets = tickets.Where(t => t.UsuarioAtribuidoId == request.UsuarioAtribuidoId.Value).ToList();
            }

            if (request.DataAberturaInicio.HasValue)
            {
                tickets = tickets.Where(t => t.DataAbertura >= request.DataAberturaInicio.Value).ToList();
            }

            if (request.DataAberturaFim.HasValue)
            {
                tickets = tickets.Where(t => t.DataAbertura <= request.DataAberturaFim.Value).ToList();
            }

            var ticketIds = tickets.Select(t => t.Id).ToList();
            var respostas = ticketIds.Any()
                ? await _unitOfWork.RespostasTicket.FindAsync(r => ticketIds.Contains(r.TicketId))
                : Enumerable.Empty<RespostaTicket>();
            var respostasLookup = respostas
                .GroupBy(r => r.TicketId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var usuarioIds = tickets.Select(t => t.UsuarioAtribuidoId ?? 0)
                .Concat(respostas.Select(r => r.UsuarioId))
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            var usuarios = usuarioIds.Any()
                ? await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id))
                : Enumerable.Empty<User>();
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);

            return tickets
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => TicketMapper.ToDto(t, clienteLookup, usuarioLookup, respostasLookup));
        }
    }
}
