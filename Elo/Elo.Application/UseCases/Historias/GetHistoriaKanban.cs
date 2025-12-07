using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Application.UseCases.HistoriaStatuses;
using Elo.Application.UseCases.HistoriaTipos;

namespace Elo.Application.UseCases.Historias;

public static class GetHistoriaKanban
{
    public class Query : IRequest<HistoriaKanbanDto>
    {
        public int? EmpresaId { get; set; }
        public int? StatusId { get; set; }
        public int? TipoId { get; set; }
        public int? ClienteId { get; set; }
        public int? ProdutoId { get; set; }
        public int? UsuarioResponsavelId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }

    public class Handler : IRequestHandler<Query, HistoriaKanbanDto>
    {
        private readonly IMediator _mediator;

        public Handler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<HistoriaKanbanDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var historias = await _mediator.Send(new GetAllHistorias.Query
            {
                EmpresaId = request.EmpresaId,
                StatusId = request.StatusId,
                TipoId = request.TipoId,
                ClienteId = request.ClienteId,
                ProdutoId = request.ProdutoId,
                UsuarioResponsavelId = request.UsuarioResponsavelId,
                DataInicio = request.DataInicio,
                DataFim = request.DataFim
            }, cancellationToken);

            var statuses = await _mediator.Send(new GetAllHistoriaStatus.Query
            {
                EmpresaId = request.EmpresaId
            }, cancellationToken);

            var tipos = await _mediator.Send(new GetAllHistoriaTipos.Query
            {
                EmpresaId = request.EmpresaId
            }, cancellationToken);

            return new HistoriaKanbanDto
            {
                Historias = historias,
                Statuses = statuses,
                Tipos = tipos
            };
        }
    }
}
