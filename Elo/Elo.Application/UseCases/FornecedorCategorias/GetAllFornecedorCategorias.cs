using MediatR;
using Elo.Application.DTOs.Fornecedor;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.FornecedorCategorias;

public static class GetAllFornecedorCategorias
{
    public class Query : IRequest<IEnumerable<FornecedorCategoriaDto>>
    {
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<FornecedorCategoriaDto>>
    {
        private readonly IFornecedorCategoriaService _service;

        public Handler(IFornecedorCategoriaService service)
        {
            _service = service;
        }

        public async Task<IEnumerable<FornecedorCategoriaDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var categorias = await _service.ObterTodasAsync(request.EmpresaId);
            return categorias.Select(c => new FornecedorCategoriaDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Ativo = c.Ativo
            });
        }
    }
}
