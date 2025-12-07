using MediatR;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.HistoriaStatuses;

public static class DeleteHistoriaStatus
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
            var status = await _unitOfWork.HistoriaStatuses.GetByIdAsync(request.Id);
            if (status == null || status.EmpresaId != request.EmpresaId)
            {
                throw new KeyNotFoundException("Status não encontrado para esta empresa.");
            }

            await _unitOfWork.HistoriaStatuses.DeleteAsync(status);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
