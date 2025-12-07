using MediatR;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.TicketTipos;

public static class DeleteTicketTipo
{
    public class Command : IRequest<Unit>
    {
        public int EmpresaId { get; set; }
        public int Id { get; set; }
    }

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var tipo = await _unitOfWork.TicketTipos.GetByIdAsync(request.Id);
            if (tipo == null || tipo.EmpresaId != request.EmpresaId)
            {
                throw new KeyNotFoundException("Tipo não encontrado para esta empresa.");
            }

            await _unitOfWork.TicketTipos.DeleteAsync(tipo);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
