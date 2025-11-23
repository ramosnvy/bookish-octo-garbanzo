using MediatR;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.ContasPagar;

public static class DeleteContaPagar
{
    public class Command : IRequest<bool>
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var conta = await _unitOfWork.ContasPagar.GetByIdAsync(request.Id) ?? throw new KeyNotFoundException("Conta não encontrada.");
            if (conta.EmpresaId != request.EmpresaId)
            {
                throw new UnauthorizedAccessException("Conta pertence a outra empresa.");
            }

            await _unitOfWork.ContasPagar.DeleteAsync(conta);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
