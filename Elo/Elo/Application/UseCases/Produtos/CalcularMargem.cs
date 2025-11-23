using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Produtos;

public static class CalcularMargem
{
    public class Command : IRequest<decimal>
    {
        public decimal ValorCusto { get; set; }
        public decimal ValorRevenda { get; set; }
    }

    public class Handler : IRequestHandler<Command, decimal>
    {
        private readonly IProdutoService _produtoService;

        public Handler(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        public Task<decimal> Handle(Command request, CancellationToken cancellationToken)
        {
            var margem = _produtoService.CalcularMargemLucro(request.ValorCusto, request.ValorRevenda);
            return Task.FromResult(margem);
        }
    }
}
