using MediatR;
using Elo.Application.Common;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Historias;

public static class GetAllHistoriasPaged
{
    public class Query : IRequest<PagedResult<HistoriaListDto>>
    {
        public int? EmpresaId { get; set; }
        public int? ClienteId { get; set; }
        public int? StatusId { get; set; }
        public int? TipoId { get; set; }
        public int? ProdutoId { get; set; }
        public int? UsuarioResponsavelId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler : IRequestHandler<Query, PagedResult<HistoriaListDto>>
    {
        private readonly IHistoriaService _historiaService;
        private readonly IPessoaService _pessoaService;
        private readonly IProdutoService _produtoService;
        private readonly IUserService _userService;
        private readonly IHistoriaStatusService _statusService;
        private readonly IHistoriaTipoService _tipoService;

        public Handler(
            IHistoriaService historiaService,
            IPessoaService pessoaService,
            IProdutoService produtoService,
            IUserService userService,
            IHistoriaStatusService statusService,
            IHistoriaTipoService tipoService)
        {
            _historiaService = historiaService;
            _pessoaService = pessoaService;
            _produtoService = produtoService;
            _userService = userService;
            _statusService = statusService;
            _tipoService = tipoService;
        }

        public async Task<PagedResult<HistoriaListDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Validar parâmetros de paginação
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

            // Fetch initial set (filtered by DB where possible)
            var historias = await _historiaService.ObterHistoriasAsync(request.EmpresaId, request.ClienteId, request.StatusId);
            var lista = historias.ToList();

            // Apply in-memory filters
            if (request.TipoId.HasValue) 
                lista = lista.Where(h => h.HistoriaTipoId == request.TipoId.Value).ToList();
            if (request.ProdutoId.HasValue) 
                lista = lista.Where(h => h.ProdutoId == request.ProdutoId.Value).ToList();
            if (request.UsuarioResponsavelId.HasValue) 
                lista = lista.Where(h => h.UsuarioResponsavelId == request.UsuarioResponsavelId.Value).ToList();
            if (request.DataInicio.HasValue) 
                lista = lista.Where(h => h.DataInicio >= request.DataInicio.Value).ToList();
            if (request.DataFim.HasValue) 
                lista = lista.Where(h => h.DataInicio <= request.DataFim.Value).ToList();

            var totalCount = lista.Count;

            if (totalCount == 0)
                return PagedResult<HistoriaListDto>.Empty(pageNumber, pageSize);

            // Apply pagination
            var pagedList = lista
                .OrderByDescending(h => h.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Fetch related data only for the current page
            var clienteIds = pagedList.Select(h => h.ClienteId).Distinct().ToList();
            var statusIds = pagedList.Select(h => h.HistoriaStatusId).Distinct().ToList();
            var tipoIds = pagedList.Select(h => h.HistoriaTipoId).Distinct().ToList();
            var produtoIds = pagedList.Select(h => h.ProdutoId).Distinct().ToList();
            var userIds = pagedList.Where(h => h.UsuarioResponsavelId.HasValue)
                .Select(h => h.UsuarioResponsavelId!.Value).Distinct().ToList();

            // Fetch all related data in parallel
            var clientesTask = _pessoaService.ObterPessoasPorIdsAsync(clienteIds, request.EmpresaId);
            var statusesTask = _statusService.ObterPorListaIdsAsync(statusIds);
            var tiposTask = _tipoService.ObterPorListaIdsAsync(tipoIds);
            var produtosTask = _produtoService.ObterProdutosPorIdsAsync(produtoIds);
            var usersTask = _userService.ObterUsuariosPorIdsAsync(userIds);

            await Task.WhenAll(clientesTask, statusesTask, tiposTask, produtosTask, usersTask);

            var clientes = await clientesTask;
            var statuses = await statusesTask;
            var tipos = await tiposTask;
            var produtos = await produtosTask;
            var users = await usersTask;

            // Create lookups
            var clienteLookup = clientes.ToDictionary(c => c.Id, c => c.Nome);
            var statusLookup = statuses.ToDictionary(s => s.Id);
            var tipoLookup = tipos.ToDictionary(t => t.Id);
            var produtoLookup = produtos.ToDictionary(p => p.Id, p => p.Nome);
            var userLookup = users.ToDictionary(u => u.Id, u => u.Nome);

            // Map to DTOs
            var items = pagedList.Select(h => new HistoriaListDto
            {
                Id = h.Id,
                ClienteId = h.ClienteId,
                ClienteNome = clienteLookup.GetValueOrDefault(h.ClienteId, string.Empty),
                ProdutoId = h.ProdutoId,
                ProdutoNome = produtoLookup.GetValueOrDefault(h.ProdutoId, string.Empty),
                HistoriaStatusId = h.HistoriaStatusId,
                HistoriaStatusNome = statusLookup.GetValueOrDefault(h.HistoriaStatusId)?.Nome ?? string.Empty,
                HistoriaStatusCor = statusLookup.GetValueOrDefault(h.HistoriaStatusId)?.Cor,
                HistoriaTipoId = h.HistoriaTipoId,
                HistoriaTipoNome = tipoLookup.GetValueOrDefault(h.HistoriaTipoId)?.Nome ?? string.Empty,
                UsuarioResponsavelId = h.UsuarioResponsavelId,
                UsuarioResponsavelNome = h.UsuarioResponsavelId.HasValue 
                    ? userLookup.GetValueOrDefault(h.UsuarioResponsavelId.Value, string.Empty) 
                    : string.Empty,
                PrevisaoDias = h.PrevisaoDias,
                DataInicio = h.DataInicio,
                DataFim = h.DataFim,
                DataFinalizacao = h.DataFinalizacao,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            }).ToList();

            return new PagedResult<HistoriaListDto>(items, totalCount, pageNumber, pageSize);
        }
    }
}
