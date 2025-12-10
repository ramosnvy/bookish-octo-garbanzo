using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Afiliados;

public static class DeleteAfiliado
{
    public class Command : IRequest
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command>
    {
        private readonly IAfiliadoService _afiliadoService;

        public Handler(IAfiliadoService afiliadoService)
        {
            _afiliadoService = afiliadoService;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _afiliadoService.DeletarAfiliadoAsync(request.Id, request.EmpresaId);
        }
    }
}
