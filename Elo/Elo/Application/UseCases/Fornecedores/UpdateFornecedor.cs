using MediatR;
using Elo.Application.DTOs.Fornecedor;
using Elo.Application.Mappers;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Fornecedores;

public static class UpdateFornecedor
{
    public class Command : IRequest<FornecedorDto>
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string Status { get; set; } = string.Empty;
        public IEnumerable<FornecedorEnderecoInputDto> Enderecos { get; set; } = Enumerable.Empty<FornecedorEnderecoInputDto>();
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, FornecedorDto>
    {
        private readonly IPessoaService _pessoaService;
        private readonly IFornecedorMapper _fornecedorMapper;

        public Handler(IPessoaService pessoaService, IFornecedorMapper fornecedorMapper)
        {
            _pessoaService = pessoaService;
            _fornecedorMapper = fornecedorMapper;
        }

        public async Task<FornecedorDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var status = Enum.TryParse<Status>(request.Status, true, out var parsedStatus) ? parsedStatus : Status.Ativo;
            var enderecoEntities = (request.Enderecos ?? Enumerable.Empty<FornecedorEnderecoInputDto>()).Select(e => new PessoaEndereco
            {
                Logradouro = e.Logradouro,
                Numero = e.Numero,
                Bairro = e.Bairro,
                Cidade = e.Cidade,
                Estado = e.Estado,
                Cep = e.Cep,
                Complemento = e.Complemento
            });

            var fornecedor = await _pessoaService.AtualizarPessoaAsync(
                request.Id,
                PessoaTipo.Fornecedor,
                request.Nome,
                request.Cnpj,
                request.Email,
                request.Telefone,
                status,
                request.CategoriaId,
                enderecoEntities,
                request.EmpresaId
            );

            return _fornecedorMapper.ToDto(fornecedor);
        }
    }
}
