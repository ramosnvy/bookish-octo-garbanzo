using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Historias;

public static class DeleteHistoria
{
    public class Command : IRequest<bool>
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IHistoriaService _historiaService;

        public Handler(IHistoriaService historiaService)
        {
            _historiaService = historiaService;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            return await _historiaService.DeletarHistoriaAsync(request.Id, request.EmpresaId);
        }
    }
}
