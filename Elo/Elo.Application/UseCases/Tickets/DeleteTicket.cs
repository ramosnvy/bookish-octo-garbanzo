using MediatR;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

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
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(request.Id);
            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket não encontrado.");
            }

            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente || cliente.EmpresaId != request.EmpresaId)
            {
                throw new UnauthorizedAccessException("Ticket não pertence à empresa informada.");
            }

            await _unitOfWork.Tickets.DeleteAsync(ticket);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
