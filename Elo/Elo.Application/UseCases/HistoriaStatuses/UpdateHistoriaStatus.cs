using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.HistoriaStatuses;

public static class UpdateHistoriaStatus
{
    public class Command : IRequest<HistoriaStatusDto>
    {
        public UpdateHistoriaStatusDto Dto { get; set; } = new();
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, HistoriaStatusDto>
    {
        private readonly IHistoriaStatusService _service;
        public Handler(IHistoriaStatusService service) => _service = service;
        public async Task<HistoriaStatusDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var status = await _service.AtualizarAsync(request.Dto.Id, request.Dto.Nome, request.Dto.Cor, request.Dto.Ordem, request.Dto.FechaHistoria, request.Dto.Ativo, request.EmpresaId);
            return new HistoriaStatusDto 
            { 
                Id = status.Id, 
                Nome = status.Nome, 
                Cor = status.Cor, 
                Ordem = status.Ordem,
                FechaHistoria = status.FechaHistoria,
                Ativo = status.Ativo
            };
        }
    }
}
