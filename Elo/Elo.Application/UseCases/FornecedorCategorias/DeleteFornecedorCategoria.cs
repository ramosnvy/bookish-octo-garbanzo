using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.FornecedorCategorias;

public static class DeleteFornecedorCategoria
{
    public class Command : IRequest<bool>
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IFornecedorCategoriaService _service;

        public Handler(IFornecedorCategoriaService service)
        {
            _service = service;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            return await _service.DeletarAsync(request.Id, request.EmpresaId);
        }
    }
}
