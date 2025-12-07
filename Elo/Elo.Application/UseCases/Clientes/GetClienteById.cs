using MediatR;
using Elo.Application.DTOs.Cliente;
using Elo.Application.Mappers;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Clientes;

public static class GetClienteById
{
    public class Query : IRequest<ClienteDto?>
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, ClienteDto?>
    {
        private readonly IPessoaService _pessoaService;
        private readonly IClienteMapper _clienteMapper;

        public Handler(IPessoaService pessoaService, IClienteMapper clienteMapper)
        {
            _pessoaService = pessoaService;
            _clienteMapper = clienteMapper;
        }

        public async Task<ClienteDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var pessoa = await _pessoaService.ObterPessoaPorIdAsync(request.Id, PessoaTipo.Cliente, request.EmpresaId);
            
            if (pessoa == null)
            {
                return null;
            }

            return _clienteMapper.ToDto(pessoa);
        }
    }
}
