using MediatR;
using Elo.Application.DTOs.Produto;
using Elo.Application.Mappers;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Produtos;

public static class GetAllProdutos
{
    public class Query : IRequest<IEnumerable<ProdutoDto>>
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? Search { get; set; }
        public bool? Ativo { get; set; }
        public decimal? ValorMinimo { get; set; }
        public decimal? ValorMaximo { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<ProdutoDto>>
    {
        private readonly IProdutoService _produtoService;
        private readonly IProdutoMapper _produtoMapper;

        public Handler(IProdutoService produtoService, IProdutoMapper produtoMapper)
        {
            _produtoService = produtoService;
            _produtoMapper = produtoMapper;
        }

        public async Task<IEnumerable<ProdutoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var produtos = await _produtoService.ObterTodosProdutosAsync(request.EmpresaId);
            return _produtoMapper.ToDtoList(produtos);
        }
    }
}

