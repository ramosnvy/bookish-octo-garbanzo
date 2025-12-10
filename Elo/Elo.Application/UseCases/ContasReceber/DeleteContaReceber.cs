using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.ContasReceber;

public static class DeleteContaReceber
{
    public class Command : IRequest<bool>
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IContaReceberService _contaReceberService;

        public Handler(IContaReceberService contaReceberService)
        {
            _contaReceberService = contaReceberService;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            return await _contaReceberService.DeletarContaReceberAsync(request.Id, request.EmpresaId);
        }
    }
}
