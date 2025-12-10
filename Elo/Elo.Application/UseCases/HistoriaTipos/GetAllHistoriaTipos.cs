using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.HistoriaTipos;

public static class GetAllHistoriaTipos
{
    public class Query : IRequest<IEnumerable<HistoriaTipoDto>>
    {
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<HistoriaTipoDto>>
    {
        private readonly IHistoriaTipoService _service;
        public Handler(IHistoriaTipoService service) => _service = service;
        public async Task<IEnumerable<HistoriaTipoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tipos = await _service.ObterTodosAsync(request.EmpresaId);
            return tipos.Select(t => new HistoriaTipoDto 
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
