using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.TicketTipos;

public static class GetAllTicketTipos
{
    public class Query : IRequest<IEnumerable<TicketTipoDto>>
    {
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<TicketTipoDto>>
    {
        private readonly ITicketTipoService _service;
        public Handler(ITicketTipoService service) => _service = service;
        public async Task<IEnumerable<TicketTipoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tipos = await _service.ObterTodosAsync(request.EmpresaId);
            return tipos.Select(t => new TicketTipoDto 
            { 
                Id = t.Id, 
                Nome = t.Nome, 
                Descricao = t.Descricao,
                Ordem = t.Ordem,
                Ativo = t.Ativo,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt 
            });
        }
    }
}
