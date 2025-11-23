using MediatR;
using Elo.Application.DTOs.Cliente;
using Elo.Application.Mappers;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Clientes;

public static class CreateCliente
{
    public class Command : IRequest<ClienteDto>
    {
        public string Nome { get; set; } = string.Empty;
        public string CnpjCpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Status { get; set; } = "Ativo";
        public IEnumerable<ClienteEnderecoInputDto> Enderecos { get; set; } = Enumerable.Empty<ClienteEnderecoInputDto>();
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, ClienteDto>
    {
        private readonly IPessoaService _pessoaService;
        private readonly IClienteMapper _clienteMapper;

        public Handler(IPessoaService pessoaService, IClienteMapper clienteMapper)
        {
            _pessoaService = pessoaService;
            _clienteMapper = clienteMapper;
        }

        public async Task<ClienteDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var status = Enum.TryParse<Status>(request.Status, true, out var parsedStatus) ? parsedStatus : Status.Ativo;

            var enderecoEntities = (request.Enderecos ?? Enumerable.Empty<ClienteEnderecoInputDto>()).Select(e => new PessoaEndereco
            {
                Logradouro = e.Logradouro,
                Numero = e.Numero,
                Bairro = e.Bairro,
                Cidade = e.Cidade,
                Estado = e.Estado,
                Cep = e.Cep,
                Complemento = e.Complemento
            });

            var pessoa = await _pessoaService.CriarPessoaAsync(
                PessoaTipo.Cliente,
                request.Nome,
                request.CnpjCpf,
                request.Email,
                request.Telefone,
                status,
                null,
                enderecoEntities,
                request.EmpresaId
            );

            return _clienteMapper.ToDto(pessoa);
        }
    }
}
