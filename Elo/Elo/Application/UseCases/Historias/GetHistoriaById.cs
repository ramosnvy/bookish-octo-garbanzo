using System.Collections.Generic;
using System.Linq;
using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces.Repositories;

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
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<HistoriaDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var historia = await _unitOfWork.Historias.GetByIdAsync(request.Id);
            if (historia == null)
            {
                return null;
            }

            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(historia.ClienteId);
            if (cliente == null || cliente.Tipo != Domain.Enums.PessoaTipo.Cliente)
            {
                return null;
            }

            if (request.EmpresaId.HasValue && cliente.EmpresaId != request.EmpresaId.Value)
            {
                return null;
            }

            var historiaProdutos = await _unitOfWork.HistoriaProdutos.FindAsync(hp => hp.HistoriaId == historia.Id);
            var produtoIds = historiaProdutos.Select(p => p.ProdutoId).Append(historia.ProdutoId).Distinct().ToList();
            var produtos = produtoIds.Any()
                ? await _unitOfWork.Produtos.FindAsync(p => produtoIds.Contains(p.Id))
                : Enumerable.Empty<Produto>();
            var moduloIds = historiaProdutos.SelectMany(p => p.ProdutoModuloIds).Distinct().ToList();
            var modulos = moduloIds.Any()
                ? await _unitOfWork.ProdutoModulos.FindAsync(m => moduloIds.Contains(m.Id))
                : Enumerable.Empty<ProdutoModulo>();

            var movimentos = await _unitOfWork.HistoriaMovimentacoes.FindAsync(m => m.HistoriaId == historia.Id);
            var usuarioIds = movimentos.Select(m => m.UsuarioId)
                .Append(historia.UsuarioResponsavelId)
                .Distinct()
                .ToList();
            var usuarios = await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id));

            var clienteLookup = new Dictionary<int, Pessoa> { { cliente.Id, cliente } };
            var produtoLookup = produtos.ToDictionary(p => p.Id, p => p);
            var moduloLookup = modulos.ToDictionary(m => m.Id, m => m);
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);
            var movimentosLookup = new Dictionary<int, List<HistoriaMovimentacao>>
            {
                { historia.Id, movimentos.ToList() }
            };
            var produtosLookup = new Dictionary<int, List<HistoriaProduto>>
            {
                { historia.Id, historiaProdutos.ToList() }
            };

            return HistoriaMapper.ToDto(
                historia,
                clienteLookup,
                produtoLookup,
                moduloLookup,
                usuarioLookup,
                produtosLookup,
                movimentosLookup);
        }
    }
}
