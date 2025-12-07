using MediatR;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Historias;

public static class DeleteHistoria
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
            var historia = await _unitOfWork.Historias.GetByIdAsync(request.Id);
            if (historia == null)
            {
                throw new KeyNotFoundException("Ação não encontrada.");
            }

            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(historia.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente || cliente.EmpresaId != request.EmpresaId)
            {
                throw new UnauthorizedAccessException("História não pertence à empresa informada.");
            }

            await _unitOfWork.Historias.DeleteAsync(historia);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
