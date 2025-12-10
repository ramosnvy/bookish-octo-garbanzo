using MediatR;
using Elo.Application.Common;
using Elo.Application.DTOs.Cliente;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Clientes;

public static class GetAllClientesPaged
{
    public class Query : IRequest<PagedResult<ClienteListDto>>
    {
        public int? EmpresaId { get; set; }
        public string? Search { get; set; }
        public Status? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler : IRequestHandler<Query, PagedResult<ClienteListDto>>
    {
        private readonly IPessoaService _pessoaService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IPessoaService pessoaService, IUnitOfWork unitOfWork)
        {
            _pessoaService = pessoaService;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<ClienteListDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Validar parâmetros de paginação
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

            var pessoas = await _pessoaService.ObterPessoasAsync(PessoaTipo.Cliente, request.EmpresaId);
            var lista = pessoas.ToList();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLower();
                lista = lista.Where(p => 
                    p.Nome.ToLower().Contains(searchLower) ||
                    (p.Documento != null && p.Documento.Contains(request.Search)) ||
                    (p.Email != null && p.Email.ToLower().Contains(searchLower))
                ).ToList();
            }

            if (request.Status.HasValue)
            {
                lista = lista.Where(p => p.Status == request.Status.Value).ToList();
            }

            var totalCount = lista.Count;

            if (totalCount == 0)
                return PagedResult<ClienteListDto>.Empty(pageNumber, pageSize);

            // Apply pagination
            var pagedList = lista
                .OrderBy(p => p.Nome)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Buscar endereços para os clientes da página atual
            var pessoaIds = pagedList.Select(p => p.Id).ToList();
            var enderecos = await _unitOfWork.PessoaEnderecos.FindAsync(e => pessoaIds.Contains(e.PessoaId));
            var enderecosGrouped = enderecos.GroupBy(e => e.PessoaId).ToDictionary(g => g.Key, g => g.ToList());

            // Map to DTOs
            var items = pagedList.Select(p =>
            {
                var pessoaEnderecos = enderecosGrouped.GetValueOrDefault(p.Id, new List<Domain.Entities.PessoaEndereco>());
                var enderecoPrincipal = pessoaEnderecos.FirstOrDefault();

                return new ClienteListDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    CnpjCpf = p.Documento,
                    Email = p.Email,
                    Telefone = p.Telefone,
                    Status = p.Status,
                    StatusNome = p.Status.ToString(),
                    CreatedAt = p.DataCadastro,
                    UpdatedAt = p.UpdatedAt,
                    QuantidadeEnderecos = pessoaEnderecos.Count,
                    CidadePrincipal = enderecoPrincipal?.Cidade,
                    EstadoPrincipal = enderecoPrincipal?.Estado
                };
            }).ToList();

            return new PagedResult<ClienteListDto>(items, totalCount, pageNumber, pageSize);
        }
    }
}
