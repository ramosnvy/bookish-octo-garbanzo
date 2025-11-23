using MediatR;
using Elo.Application.DTOs.Fornecedor;
using Elo.Application.Mappers;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Fornecedores;

public static class GetFornecedorById
{
    public class Query : IRequest<FornecedorDto?>
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, FornecedorDto?>
    {
        private readonly IPessoaService _pessoaService;
        private readonly IFornecedorMapper _fornecedorMapper;

        public Handler(IPessoaService pessoaService, IFornecedorMapper fornecedorMapper)
        {
            _pessoaService = pessoaService;
            _fornecedorMapper = fornecedorMapper;
        }

        public async Task<FornecedorDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var pessoa = await _pessoaService.ObterPessoaPorIdAsync(request.Id, PessoaTipo.Fornecedor, request.EmpresaId);
            if (pessoa == null)
            {
                return null;
            }

            return _fornecedorMapper.ToDto(pessoa);
        }
    }
}

