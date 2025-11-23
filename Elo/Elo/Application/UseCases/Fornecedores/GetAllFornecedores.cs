using MediatR;
using Elo.Application.DTOs.Fornecedor;
using Elo.Application.Mappers;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Fornecedores;

public static class GetAllFornecedores
{
    public class Query : IRequest<IEnumerable<FornecedorDto>>
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? Search { get; set; }
        public string? Categoria { get; set; }
        public string? Status { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<FornecedorDto>>
    {
        private readonly IPessoaService _pessoaService;
        private readonly IFornecedorMapper _fornecedorMapper;

        public Handler(IPessoaService pessoaService, IFornecedorMapper fornecedorMapper)
        {
            _pessoaService = pessoaService;
            _fornecedorMapper = fornecedorMapper;
        }

        public async Task<IEnumerable<FornecedorDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var pessoas = await _pessoaService.ObterPessoasAsync(PessoaTipo.Fornecedor, request.EmpresaId);
            return _fornecedorMapper.ToDtoList(pessoas);
        }
    }
}

