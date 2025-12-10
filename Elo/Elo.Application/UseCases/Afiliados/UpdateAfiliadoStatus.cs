using Elo.Application.DTOs.Afiliado;
using Elo.Application.Mappers;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;
using MediatR;
using System.Text.Json.Serialization;

namespace Elo.Application.UseCases.Afiliados;

public static class UpdateAfiliadoStatus
{
    public class Command : IRequest<AfiliadoDto>
    {
        public int Id { get; set; }
        public Status Status { get; set; }
        [JsonIgnore]
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, AfiliadoDto>
    {
        private readonly IAfiliadoService _service;
        private readonly IAfiliadoMapper _mapper;

        public Handler(IAfiliadoService service, IAfiliadoMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        public async Task<AfiliadoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var afiliado = await _service.AtualizarStatusAfiliadoAsync(request.Id, request.Status, request.EmpresaId);
            return _mapper.ToDto(afiliado);
        }
    }
}
