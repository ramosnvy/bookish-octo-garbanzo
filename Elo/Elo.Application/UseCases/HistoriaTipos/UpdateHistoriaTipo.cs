using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.HistoriaTipos;

public static class UpdateHistoriaTipo
{
    public class Command : IRequest<HistoriaTipoDto>
    {
        public UpdateHistoriaTipoDto Dto { get; set; } = new();
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, HistoriaTipoDto>
    {
        private readonly IHistoriaTipoService _service;
        public Handler(IHistoriaTipoService service) => _service = service;
        public async Task<HistoriaTipoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var tipo = await _service.AtualizarAsync(request.Dto.Id, request.Dto.Nome, request.Dto.Descricao, request.Dto.Ordem, request.Dto.Ativo, request.EmpresaId);
            return new HistoriaTipoDto 
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
