using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.TicketTipos;

public static class DeleteTicketTipo
{
    public class Command : IRequest { public int Id { get; set; } public int? EmpresaId { get; set; } }
    public class Handler : IRequestHandler<Command>
    {
        private readonly ITicketTipoService _service;
        public Handler(ITicketTipoService service) => _service = service;
        public async Task Handle(Command request, CancellationToken cancellationToken) => await _service.DeletarAsync(request.Id);
    }
}
