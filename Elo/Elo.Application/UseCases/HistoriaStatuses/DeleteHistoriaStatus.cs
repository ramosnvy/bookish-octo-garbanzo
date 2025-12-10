using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.HistoriaStatuses;

public static class DeleteHistoriaStatus
{
    public class Command : IRequest { public int Id { get; set; } public int? EmpresaId { get; set; } }
    public class Handler : IRequestHandler<Command>
    {
        private readonly IHistoriaStatusService _service;
        public Handler(IHistoriaStatusService service) => _service = service;
        public async Task Handle(Command request, CancellationToken cancellationToken) => await _service.DeletarAsync(request.Id);
    }
}
