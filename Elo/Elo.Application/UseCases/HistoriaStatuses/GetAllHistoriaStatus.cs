using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.HistoriaStatuses;

public static class GetAllHistoriaStatus
{
    public class Query : IRequest<IEnumerable<HistoriaStatusDto>>
    {
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<HistoriaStatusDto>>
    {
        private readonly IHistoriaStatusService _service;
        public Handler(IHistoriaStatusService service) => _service = service;
        public async Task<IEnumerable<HistoriaStatusDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var statuses = await _service.ObterTodosAsync(request.EmpresaId);
            return statuses.Select(s => new HistoriaStatusDto 
            { 
                Id = s.Id, 
                Nome = s.Nome, 
                Descricao = s.Descricao,
                Cor = s.Cor, 
                FechaHistoria = s.FechaHistoria,
                Ordem = s.Ordem,
                Ativo = s.Ativo,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt 
            });
        }
    }
}
