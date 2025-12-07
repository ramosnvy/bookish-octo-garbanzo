using MediatR;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.HistoriaTipos;

public static class DeleteHistoriaTipo
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
            var tipo = await _unitOfWork.HistoriaTipos.GetByIdAsync(request.Id);
            if (tipo == null || tipo.EmpresaId != request.EmpresaId)
            {
                throw new KeyNotFoundException("Tipo não encontrado para esta empresa.");
            }

            await _unitOfWork.HistoriaTipos.DeleteAsync(tipo);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
