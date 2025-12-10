using MediatR;
using Elo.Application.DTOs.Afiliado;
using Elo.Application.Mappers;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Afiliados;

public static class GetAfiliadoById
{
    public class Query : IRequest<AfiliadoDto?>
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, AfiliadoDto?>
    {
        private readonly IAfiliadoService _afiliadoService;
        private readonly IAfiliadoMapper _mapper;

        public Handler(IAfiliadoService afiliadoService, IAfiliadoMapper mapper)
        {
            _afiliadoService = afiliadoService;
            _mapper = mapper;
        }

        public async Task<AfiliadoDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!request.EmpresaId.HasValue)
                throw new ArgumentException("EmpresaId é obrigatório");

            var afiliado = await _afiliadoService.ObterAfiliadoPorIdAsync(request.Id, request.EmpresaId.Value);
            
            if (afiliado == null)
                return null;

            return _mapper.ToDto(afiliado);
        }
    }
}
