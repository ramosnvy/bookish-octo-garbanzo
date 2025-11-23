using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Produtos;

public static class DeleteProduto
{
    public class Command : IRequest<bool>
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IProdutoService _produtoService;

        public Handler(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            return await _produtoService.DeletarProdutoAsync(request.Id, request.EmpresaId);
        }
    }
}

