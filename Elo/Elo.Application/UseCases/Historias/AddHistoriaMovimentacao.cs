using MediatR;
using Elo.Application.DTOs;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Historias;

public static class AddHistoriaMovimentacao
{
    public class Command : IRequest<HistoriaMovimentacaoDto>
    {
        public int HistoriaId { get; set; }
        public int EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public CreateHistoriaMovimentacaoDto Dto { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, HistoriaMovimentacaoDto>
    {
        private readonly IHistoriaService _historiaService;

        public Handler(IHistoriaService historiaService)
        {
            _historiaService = historiaService;
        }

        public async Task<HistoriaMovimentacaoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            // Note: statusAnteriorId? The service takes statusAnteriorId.
            // But if we don't know it from DTO, we should fetch it?
            // Service expects it.
            // If DTO doesn't have it, we fetch story first.
            
            var historia = await _historiaService.ObterHistoriaPorIdAsync(request.HistoriaId, request.EmpresaId);
            if (historia == null) throw new KeyNotFoundException("História não encontrada.");

            var movimentacao = await _historiaService.AdicionarMovimentacaoAsync(
                request.HistoriaId,
                historia.HistoriaStatusId, // Status Anterior is current status before update
                request.Dto.StatusNovoId,
                request.UsuarioId,
                request.Dto.Observacoes,
                request.EmpresaId
            );

            return new HistoriaMovimentacaoDto
            {
                Id = movimentacao.Id,
                HistoriaId = movimentacao.HistoriaId,
                StatusAnteriorId = movimentacao.StatusAnteriorId,
                StatusNovoId = movimentacao.StatusNovoId,
                UsuarioId = movimentacao.UsuarioId,
                Observacoes = movimentacao.Observacoes,
                DataMovimentacao = movimentacao.DataMovimentacao
            };
        }
    }
}
