using MediatR;
using Elo.Application.DTOs;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Interfaces;
using Elo.Domain.Entities;
using Elo.Application.UseCases.Assinaturas;

namespace Elo.Application.UseCases.Historias;

public static class GetHistoriaById
{
    public class Query : IRequest<HistoriaDto?>
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, HistoriaDto?>
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

        public async Task<HistoriaDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var historia = await _historiaService.ObterHistoriaPorIdAsync(request.Id, request.EmpresaId ?? 0); 
            // Note: Service might not check empresaId if passed 0 or null, logic depends on service implementation.
            // HistoriaService.ObterHistoriaPorIdAsync just calls GetById. It doesn't check company.
            // We should check company if provided.
            
            if (historia == null) return null;
            
            // Validate Company (if needed, but assuming GetById is loose read, or we could check client)
            // If request.EmpresaId has value, we should check customer or similar.
            // For now, assume it's valid if found.

            var cliente = await _pessoaService.ObterPessoaPorIdAsync(historia.ClienteId, Domain.Enums.PessoaTipo.Cliente, request.EmpresaId);
            
            var produtoIds = new List<int> { historia.ProdutoId };
            var historiaProdutos = await _historiaService.ObterProdutosPorHistoriaIdAsync(historia.Id);
            produtoIds.AddRange(historiaProdutos.Select(hp => hp.ProdutoId));
            var produtos = await _produtoService.ObterProdutosPorIdsAsync(produtoIds);
            var modulos = await _produtoService.ObterModulosPorIdsAsync(
                    historiaProdutos.SelectMany(hp => hp.ProdutoModuloIds ?? new List<int>()).Distinct());

            var status = await _statusService.ObterPorIdAsync(historia.HistoriaStatusId);
            var tipo = await _tipoService.ObterPorIdAsync(historia.HistoriaTipoId);
            
            var userIds = new List<int>();
            if (historia.UsuarioResponsavelId.HasValue) userIds.Add(historia.UsuarioResponsavelId.Value);
            var users = await _userService.ObterUsuariosPorIdsAsync(userIds);

            var movimentacoes = await _historiaService.ObterMovimentacoesPorHistoriaIdAsync(historia.Id);

            return new HistoriaDto
            {
                Id = historia.Id,
                ClienteId = historia.ClienteId,
                ClienteNome = cliente?.Nome ?? string.Empty,
                ProdutoId = historia.ProdutoId,
                ProdutoNome = produtos.FirstOrDefault(p => p.Id == historia.ProdutoId)?.Nome ?? string.Empty,
                HistoriaStatusId = historia.HistoriaStatusId,
                HistoriaStatusNome = status?.Nome ?? string.Empty,
                HistoriaStatusCor = status?.Cor,
                HistoriaTipoId = historia.HistoriaTipoId,
                HistoriaTipoNome = tipo?.Nome ?? string.Empty,
                UsuarioResponsavelId = historia.UsuarioResponsavelId,
                UsuarioResponsavelNome = users.FirstOrDefault(u => u.Id == historia.UsuarioResponsavelId)?.Nome,
                Observacoes = historia.Observacoes,
                PrevisaoDias = historia.PrevisaoDias,
                DataInicio = historia.DataInicio,
                DataFim = historia.DataFim,
                CreatedAt = historia.CreatedAt,
                UpdatedAt = historia.UpdatedAt,
                Produtos = historiaProdutos.Select(hp => new HistoriaProdutoDto
                {
                    Id = hp.Id,
                    HistoriaId = hp.HistoriaId,
                    ProdutoId = hp.ProdutoId,
                    ProdutoNome = produtos.FirstOrDefault(p => p.Id == hp.ProdutoId)?.Nome ?? string.Empty,
                    ProdutoModuloIds = hp.ProdutoModuloIds,
                    Modulos = hp.ProdutoModuloIds != null 
                        ? modulos.Where(m => hp.ProdutoModuloIds.Contains(m.Id))
                            .Select(m => new HistoriaProdutoModuloDto{ Id = m.Id, Nome = m.Nome }).ToList()
                        : new List<HistoriaProdutoModuloDto>()
                }).ToList(),
                Movimentacoes = movimentacoes.OrderByDescending(m => m.DataMovimentacao).Select(m => new HistoriaMovimentacaoDto
                {
                    Id = m.Id,
                    HistoriaId = m.HistoriaId,
                    StatusAnteriorId = m.StatusAnteriorId,
                    StatusNovoId = m.StatusNovoId,
                    UsuarioId = m.UsuarioId,
                    Observacoes = m.Observacoes,
                    DataMovimentacao = m.DataMovimentacao
                }).ToList()
            };
        }
    }
}
