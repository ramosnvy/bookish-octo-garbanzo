using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Tickets;

public static class DeleteTicket
{
    public class Command : IRequest
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly ITicketService _ticketService;

        public Handler(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _ticketService.DeletarTicketAsync(request.Id, request.EmpresaId);
        }
    }
}
