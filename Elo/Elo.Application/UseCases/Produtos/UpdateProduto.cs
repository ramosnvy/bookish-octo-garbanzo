using MediatR;
using Elo.Application.DTOs.Produto;
using Elo.Application.Mappers;
using Elo.Domain.Interfaces;
using Elo.Domain.Models;

namespace Elo.Application.UseCases.Produtos;

public static class UpdateProduto
{
    public class Command : IRequest<ProdutoDto>
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorCusto { get; set; }
        public decimal ValorRevenda { get; set; }
        public bool Ativo { get; set; }
        public int? FornecedorId { get; set; }
        public IEnumerable<ProdutoModuloInputDto> Modulos { get; set; } = new List<ProdutoModuloInputDto>();
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, ProdutoDto>
    {
        private readonly IProdutoService _produtoService;
        private readonly IProdutoMapper _produtoMapper;

        public Handler(IProdutoService produtoService, IProdutoMapper produtoMapper)
        {
            _produtoService = produtoService;
            _produtoMapper = produtoMapper;
        }

        public async Task<ProdutoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var produto = await _produtoService.AtualizarProdutoAsync(
                request.Id,
                request.Nome,
                request.Descricao,
                request.ValorCusto,
                request.ValorRevenda,
                request.Ativo,
                request.FornecedorId,
                MapModulos(request.Modulos),
                request.EmpresaId
            );

            return _produtoMapper.ToDto(produto);
        }

        private IEnumerable<ProdutoModuloInput> MapModulos(IEnumerable<ProdutoModuloInputDto> modulos)
        {
            return (modulos ?? Enumerable.Empty<ProdutoModuloInputDto>()).Select(m => new ProdutoModuloInput
            {
                Nome = m.Nome,
                Descricao = m.Descricao,
                ValorAdicional = m.ValorAdicional,
                CustoAdicional = m.CustoAdicional,
                Ativo = m.Ativo
            });
        }
    }
}

