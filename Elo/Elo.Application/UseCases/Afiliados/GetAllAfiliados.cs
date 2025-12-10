using MediatR;
using Elo.Application.DTOs.Afiliado;
using Elo.Application.Mappers;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Afiliados;

public static class GetAllAfiliados
{
    public class Query : IRequest<IEnumerable<AfiliadoDto>>
    {
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<AfiliadoDto>>
    {
        private readonly IAfiliadoService _afiliadoService;
        private readonly IAfiliadoMapper _mapper;

        public Handler(IAfiliadoService afiliadoService, IAfiliadoMapper mapper)
        {
            _afiliadoService = afiliadoService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AfiliadoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var afiliados = await _afiliadoService.ObterAfiliadosAsync(request.EmpresaId);
            return _mapper.ToDtoList(afiliados);
        }
    }
}
