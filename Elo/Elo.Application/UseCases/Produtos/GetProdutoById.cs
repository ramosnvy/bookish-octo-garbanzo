using MediatR;
using Elo.Application.DTOs.Produto;
using Elo.Application.Mappers;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Produtos;

public static class GetProdutoById
{
    public class Query : IRequest<ProdutoDto?>
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, ProdutoDto?>
    {
        private readonly IProdutoService _produtoService;
        private readonly IProdutoMapper _produtoMapper;

        public Handler(IProdutoService produtoService, IProdutoMapper produtoMapper)
        {
            _produtoService = produtoService;
            _produtoMapper = produtoMapper;
        }

        public async Task<ProdutoDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var produto = await _produtoService.ObterProdutoPorIdAsync(request.Id, request.EmpresaId);
            
            if (produto == null)
            {
                return null;
            }

            return _produtoMapper.ToDto(produto);
        }
    }
}
