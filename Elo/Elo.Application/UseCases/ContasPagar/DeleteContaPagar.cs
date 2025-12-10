using MediatR;
using Elo.Domain.Interfaces;

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
        private readonly IContaPagarService _contaPagarService;

        public Handler(IContaPagarService contaPagarService)
        {
            _contaPagarService = contaPagarService;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            return await _contaPagarService.DeletarContaPagarAsync(request.Id, request.EmpresaId);
        }
    }
}
