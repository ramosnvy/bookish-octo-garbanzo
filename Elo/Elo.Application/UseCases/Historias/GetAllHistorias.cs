using MediatR;
using Elo.Application.DTOs;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Interfaces;
using Elo.Domain.Entities;
using Elo.Application.UseCases.Assinaturas;

namespace Elo.Application.UseCases.Historias;

public static class GetAllHistorias
{
    public class Query : IRequest<IEnumerable<HistoriaDto>>
    {
        public int? EmpresaId { get; set; }
        public int? ClienteId { get; set; }
        public int? StatusId { get; set; }
        public int? TipoId { get; set; }
        public int? ProdutoId { get; set; }
        public int? UsuarioResponsavelId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<HistoriaDto>>
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

        public async Task<IEnumerable<HistoriaDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch initial set (filtered by DB where possible)
            var historias = await _historiaService.ObterHistoriasAsync(request.EmpresaId, request.ClienteId, request.StatusId);
            var lista = historias.ToList();

            // Apply in-memory filters
            if (request.TipoId.HasValue) lista = lista.Where(h => h.HistoriaTipoId == request.TipoId.Value).ToList();
            if (request.ProdutoId.HasValue) lista = lista.Where(h => h.ProdutoId == request.ProdutoId.Value).ToList();
            if (request.UsuarioResponsavelId.HasValue) lista = lista.Where(h => h.UsuarioResponsavelId == request.UsuarioResponsavelId.Value).ToList();
            if (request.DataInicio.HasValue) lista = lista.Where(h => h.DataInicio >= request.DataInicio.Value).ToList();
            if (request.DataFim.HasValue) lista = lista.Where(h => h.DataInicio <= request.DataFim.Value).ToList();
            
            if (!lista.Any()) return Enumerable.Empty<HistoriaDto>();

            var historiaIds = lista.Select(h => h.Id).ToList();

            // Fetch relations sequentially to avoid DbContext concurrency issues
            var historiaProdutos = (await _historiaService.ObterProdutosPorListaIdsAsync(historiaIds)).ToList();
            var movimentacoes = (await _historiaService.ObterMovimentacoesPorListaIdsAsync(historiaIds)).ToList();
            
            var clienteIds = lista.Select(h => h.ClienteId).Distinct().ToList();
            var statusIds = lista.Select(h => h.HistoriaStatusId).Distinct().ToList();
            var tipoIds = lista.Select(h => h.HistoriaTipoId).Distinct().ToList();
            var userIds = lista.Where(h => h.UsuarioResponsavelId.HasValue).Select(h => h.UsuarioResponsavelId!.Value).Distinct().ToList();

            var produtoIds = lista.Select(h => h.ProdutoId).Union(historiaProdutos.Select(hp => hp.ProdutoId)).Distinct().ToList();
            var moduloIds = historiaProdutos.SelectMany(hp => hp.ProdutoModuloIds ?? new List<int>()).Distinct().ToList();

            var clientes = await _pessoaService.ObterPessoasPorIdsAsync(clienteIds, request.EmpresaId);
            var statuses = await _statusService.ObterPorListaIdsAsync(statusIds);
            var tipos = await _tipoService.ObterPorListaIdsAsync(tipoIds);
            var users = await _userService.ObterUsuariosPorIdsAsync(userIds);
            var produtos = await _produtoService.ObterProdutosPorIdsAsync(produtoIds);
            var modulos = await _produtoService.ObterModulosPorIdsAsync(moduloIds);

            var clienteLookup = clientes.ToDictionary(c => c.Id, c => c.Nome);
            var statusLookup = statuses.ToDictionary(s => s.Id, s => s);
            var tipoLookup = tipos.ToDictionary(t => t.Id, t => t);
            var userLookup = users.ToDictionary(u => u.Id, u => u.Nome);
            var produtoLookup = produtos.ToDictionary(p => p.Id, p => p.Nome);
            var moduloLookup = modulos.ToDictionary(m => m.Id, m => m);

            var hpLookup = historiaProdutos.GroupBy(hp => hp.HistoriaId).ToDictionary(g => g.Key, g => g.ToList());
            var movLookup = movimentacoes.GroupBy(m => m.HistoriaId).ToDictionary(g => g.Key, g => g.ToList());

            return lista.Select(h => new HistoriaDto
            {
                Id = h.Id,
                ClienteId = h.ClienteId,
                ClienteNome = clienteLookup.ContainsKey(h.ClienteId) ? clienteLookup[h.ClienteId] : string.Empty,
                ProdutoId = h.ProdutoId,
                ProdutoNome = produtoLookup.ContainsKey(h.ProdutoId) ? produtoLookup[h.ProdutoId] : string.Empty,
                HistoriaStatusId = h.HistoriaStatusId,
                HistoriaStatusNome = statusLookup.ContainsKey(h.HistoriaStatusId) ? statusLookup[h.HistoriaStatusId].Nome : string.Empty,
                HistoriaStatusCor = statusLookup.ContainsKey(h.HistoriaStatusId) ? statusLookup[h.HistoriaStatusId].Cor : null,
                StatusFechaHistoria = statusLookup.ContainsKey(h.HistoriaStatusId) ? statusLookup[h.HistoriaStatusId].FechaHistoria : false,
                HistoriaTipoId = h.HistoriaTipoId,
                HistoriaTipoNome = tipoLookup.ContainsKey(h.HistoriaTipoId) ? tipoLookup[h.HistoriaTipoId].Nome : string.Empty,
                UsuarioResponsavelId = h.UsuarioResponsavelId,
                UsuarioResponsavelNome = h.UsuarioResponsavelId.HasValue && userLookup.ContainsKey(h.UsuarioResponsavelId.Value) ? userLookup[h.UsuarioResponsavelId.Value] : string.Empty,
                Observacoes = h.Observacoes,
                PrevisaoDias = h.PrevisaoDias,
                DataInicio = h.DataInicio,
                DataFim = h.DataFim,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt,
                Produtos = hpLookup.ContainsKey(h.Id) 
                    ? hpLookup[h.Id].Select(hp => new HistoriaProdutoDto
                    {
                        Id = hp.Id,
                        HistoriaId = hp.HistoriaId,
                        ProdutoId = hp.ProdutoId,
                        ProdutoNome = produtoLookup.ContainsKey(hp.ProdutoId) ? produtoLookup[hp.ProdutoId] : string.Empty,
                        ProdutoModuloIds = hp.ProdutoModuloIds,
                        Modulos = hp.ProdutoModuloIds != null 
                            ? hp.ProdutoModuloIds.Where(mid => moduloLookup.ContainsKey(mid))
                                .Select(mid => new HistoriaProdutoModuloDto{ Id = mid, Nome = moduloLookup[mid].Nome }).ToList()
                            : new List<HistoriaProdutoModuloDto>()
                    }).ToList()
                    : new List<HistoriaProdutoDto>(),
                Movimentacoes = movLookup.ContainsKey(h.Id)
                    ? movLookup[h.Id].OrderByDescending(m => m.DataMovimentacao).Select(m => new HistoriaMovimentacaoDto
                    {
                        Id = m.Id,
                        HistoriaId = m.HistoriaId,
                        StatusAnteriorId = m.StatusAnteriorId,
                        StatusNovoId = m.StatusNovoId,
                        UsuarioId = m.UsuarioId,
                        Observacoes = m.Observacoes,
                        DataMovimentacao = m.DataMovimentacao
                    }).ToList()
                    : new List<HistoriaMovimentacaoDto>()
            });
        }
    }
}
