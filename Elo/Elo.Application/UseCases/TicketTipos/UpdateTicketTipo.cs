using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.TicketTipos;

public static class UpdateTicketTipo
{
    public class Command : IRequest<TicketTipoDto>
    {
        public UpdateTicketTipoDto Dto { get; set; } = new();
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, TicketTipoDto>
    {
        private readonly ITicketTipoService _service;
        public Handler(ITicketTipoService service) => _service = service;
        public async Task<TicketTipoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var tipo = await _service.AtualizarAsync(request.Dto.Id, request.Dto.Nome, request.Dto.Descricao, request.Dto.Ordem, request.Dto.Ativo, request.EmpresaId);
            return new TicketTipoDto 
            { 
                Id = tipo.Id, 
                Nome = tipo.Nome, 
                Descricao = tipo.Descricao,
                Ordem = tipo.Ordem,
                Ativo = tipo.Ativo
            };
        }
    }
}
