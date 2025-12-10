using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.HistoriaTipos;

public static class DeleteHistoriaTipo
{
    public class Command : IRequest
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly IHistoriaTipoService _service;

        public Handler(IHistoriaTipoService service)
        {
            _service = service;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _service.DeletarAsync(request.Id);
        }
    }
}
