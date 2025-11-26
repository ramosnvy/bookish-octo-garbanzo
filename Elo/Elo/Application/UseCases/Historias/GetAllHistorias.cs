using System.Collections.Generic;
using System.Linq;
using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Historias;

public static class GetAllHistorias
{
    public class Query : IRequest<IEnumerable<HistoriaDto>>
    {
        public int? EmpresaId { get; set; }
        public HistoriaStatus? Status { get; set; }
        public HistoriaTipo? Tipo { get; set; }
        public int? ClienteId { get; set; }
        public int? ProdutoId { get; set; }
        public int? UsuarioResponsavelId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<HistoriaDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<HistoriaDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var clientes = await _unitOfWork.Pessoas.FindAsync(p =>
                p.Tipo == PessoaTipo.Cliente &&
                (!request.EmpresaId.HasValue || p.EmpresaId == request.EmpresaId.Value));
            var clienteLookup = clientes.ToDictionary(c => c.Id, c => c);

            var historias = (await _unitOfWork.Historias.GetAllAsync())
                .Where(i => !request.EmpresaId.HasValue || clienteLookup.ContainsKey(i.ClienteId))
                .ToList();

            if (request.Status.HasValue)
            {
                historias = historias.Where(i => i.Status == request.Status.Value).ToList();
            }

            if (request.Tipo.HasValue)
            {
                historias = historias.Where(i => i.Tipo == request.Tipo.Value).ToList();
            }

            if (request.ClienteId.HasValue)
            {
                historias = historias.Where(i => i.ClienteId == request.ClienteId.Value).ToList();
            }

            if (request.UsuarioResponsavelId.HasValue)
            {
                historias = historias.Where(i => i.UsuarioResponsavelId == request.UsuarioResponsavelId.Value).ToList();
            }

            if (request.DataInicio.HasValue)
            {
                historias = historias.Where(i => i.DataInicio >= request.DataInicio.Value).ToList();
            }

            if (request.DataFim.HasValue)
            {
                historias = historias.Where(i => i.DataFinalizacao.HasValue && i.DataFinalizacao.Value <= request.DataFim.Value).ToList();
            }

            var historiaIds = historias.Select(i => i.Id).ToList();
            var historiaProdutos = historiaIds.Any()
                ? await _unitOfWork.HistoriaProdutos.FindAsync(hp => historiaIds.Contains(hp.HistoriaId))
                : Enumerable.Empty<HistoriaProduto>();
            var historiaProdutosLookup = historiaProdutos
                .GroupBy(hp => hp.HistoriaId)
                .ToDictionary(g => g.Key, g => g.ToList());
            foreach (var id in historiaIds)
            {
                if (!historiaProdutosLookup.ContainsKey(id))
                {
                    historiaProdutosLookup[id] = new List<HistoriaProduto>();
                }
            }

            if (request.ProdutoId.HasValue)
            {
                var idsComProduto = historiaProdutos
                    .Where(hp => hp.ProdutoId == request.ProdutoId.Value)
                    .Select(hp => hp.HistoriaId)
                    .ToHashSet();
                historias = historias.Where(i => idsComProduto.Contains(i.Id)).ToList();
                historiaIds = historias.Select(i => i.Id).ToList();
                historiaProdutosLookup = historiaProdutosLookup
                    .Where(pair => historiaIds.Contains(pair.Key))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                foreach (var id in historiaIds)
                {
                    if (!historiaProdutosLookup.ContainsKey(id))
                    {
                        historiaProdutosLookup[id] = new List<HistoriaProduto>();
                    }
                }
            }

            var produtosAssociados = historiaProdutosLookup.SelectMany(pair => pair.Value).ToList();
            var produtoIds = produtosAssociados
                .Select(p => p.ProdutoId)
                .Concat(historias.Select(i => i.ProdutoId))
                .Distinct()
                .ToList();
            var produtos = produtoIds.Any()
                ? await _unitOfWork.Produtos.FindAsync(p => produtoIds.Contains(p.Id) && (!request.EmpresaId.HasValue || p.EmpresaId == request.EmpresaId.Value))
                : Enumerable.Empty<Produto>();
            var produtoLookup = produtos.ToDictionary(p => p.Id, p => p);

            var moduloIds = produtosAssociados.SelectMany(p => p.ProdutoModuloIds).Distinct().ToList();
            var modulos = moduloIds.Any()
                ? await _unitOfWork.ProdutoModulos.FindAsync(h => moduloIds.Contains(h.Id))
                : Enumerable.Empty<ProdutoModulo>();
            var moduloLookup = modulos.ToDictionary(m => m.Id, m => m);

            var movimentos = historiaIds.Any()
                ? await _unitOfWork.HistoriaMovimentacoes.FindAsync(m => historiaIds.Contains(m.HistoriaId))
                : Enumerable.Empty<HistoriaMovimentacao>();
            var movimentosLookup = movimentos
                .GroupBy(m => m.HistoriaId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var usuarioIds = historias.Select(i => i.UsuarioResponsavelId)
                .Concat(movimentos.Select(m => m.UsuarioId))
                .Distinct()
                .ToList();
            var usuarios = usuarioIds.Any()
                ? await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id))
                : Enumerable.Empty<User>();
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);

            return historias
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => HistoriaMapper.ToDto(
                    i,
                    clienteLookup,
                    produtoLookup,
                    moduloLookup,
                    usuarioLookup,
                    historiaProdutosLookup,
                    movimentosLookup));
        }
    }
}
