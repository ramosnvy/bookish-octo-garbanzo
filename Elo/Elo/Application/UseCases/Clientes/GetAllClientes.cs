using MediatR;
using Elo.Application.DTOs.Cliente;
using Elo.Application.Mappers;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Clientes;

public static class GetAllClientes
{
    public class Query : IRequest<IEnumerable<ClienteDto>>
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? Search { get; set; }
        public string? Status { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<ClienteDto>>
    {
        private readonly IPessoaService _pessoaService;
        private readonly IClienteMapper _clienteMapper;

        public Handler(IPessoaService pessoaService, IClienteMapper clienteMapper)
        {
            _pessoaService = pessoaService;
            _clienteMapper = clienteMapper;
        }

        public async Task<IEnumerable<ClienteDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var pessoas = await _pessoaService.ObterPessoasAsync(PessoaTipo.Cliente, request.EmpresaId);
            return _clienteMapper.ToDtoList(pessoas);
        }
    }
}
