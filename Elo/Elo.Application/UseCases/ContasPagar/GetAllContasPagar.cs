using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.ContasPagar;

public static class GetAllContasPagar
{
    public class Query : IRequest<IEnumerable<ContaPagarDto>>
    {
        public int? EmpresaId { get; set; }
        public ContaStatus? Status { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<ContaPagarDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ContaPagarDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            IEnumerable<Domain.Entities.ContaPagar> contas;
            if (request.EmpresaId.HasValue)
            {
                contas = await _unitOfWork.ContasPagar.FindAsync(c => c.EmpresaId == request.EmpresaId.Value);
            }
            else
            {
                contas = await _unitOfWork.ContasPagar.GetAllAsync();
            }

            var lista = contas.ToList();

            if (request.Status.HasValue)
            {
                lista = lista.Where(c => c.Status == request.Status.Value).ToList();
            }

            if (request.DataInicial.HasValue)
            {
                lista = lista.Where(c => c.DataVencimento >= request.DataInicial.Value).ToList();
            }

            if (request.DataFinal.HasValue)
            {
                lista = lista.Where(c => c.DataVencimento <= request.DataFinal.Value).ToList();
            }

            var fornecedoresIds = lista.Select(c => c.FornecedorId).Distinct().ToList();
            var fornecedores = fornecedoresIds.Any()
                ? await _unitOfWork.Pessoas.FindAsync(p => fornecedoresIds.Contains(p.Id))
                : Enumerable.Empty<Domain.Entities.Pessoa>();
            var fornecedorLookup = fornecedores.ToDictionary(f => f.Id, f => f.Nome);

            var contaIds = lista.Select(c => c.Id).ToList();
            var itens = contaIds.Any()
                ? await _unitOfWork.ContaPagarItens.FindAsync(i => contaIds.Contains(i.ContaPagarId))
                : Enumerable.Empty<Domain.Entities.ContaPagarItem>();
            var parcelas = contaIds.Any()
                ? await _unitOfWork.ContaPagarParcelas.FindAsync(p => contaIds.Contains(p.ContaPagarId))
                : Enumerable.Empty<Domain.Entities.ContaPagarParcela>();
            var itensLookup = itens.GroupBy(i => i.ContaPagarId).ToDictionary(g => g.Key, g => g.ToList());
            var parcelasLookup = parcelas.GroupBy(p => p.ContaPagarId).ToDictionary(g => g.Key, g => g.OrderBy(pi => pi.Numero).ToList());

            return lista.OrderByDescending(c => c.CreatedAt).Select(c => new ContaPagarDto
            {
                Id = c.Id,
                EmpresaId = c.EmpresaId,
                FornecedorId = c.FornecedorId,
                FornecedorNome = fornecedorLookup.TryGetValue(c.FornecedorId, out var nome) ? nome : string.Empty,
                Descricao = c.Descricao,
                Valor = c.Valor,
                DataVencimento = c.DataVencimento,
                DataPagamento = c.DataPagamento,
                Status = c.Status,
                Categoria = c.Categoria,
                IsRecorrente = c.IsRecorrente,
                TotalParcelas = c.TotalParcelas,
                IntervaloDias = c.IntervaloDias,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Itens = itensLookup.TryGetValue(c.Id, out var contasItens)
                    ? contasItens.Select(i => new ContaPagarItemDto
                    {
                        Id = i.Id,
                        ContaPagarId = i.ContaPagarId,
                        Descricao = i.Descricao,
                        Valor = i.Valor,
                        ProdutoId = i.ProdutoId,
                        ProdutoModuloId = i.ProdutoModuloIds?.FirstOrDefault(),
                        ProdutoModuloIds = i.ProdutoModuloIds ?? new List<int>()
                    })
                    : Enumerable.Empty<ContaPagarItemDto>(),
                Parcelas = parcelasLookup.TryGetValue(c.Id, out var contaParcelas)
                    ? contaParcelas.Select(p => new ContaPagarParcelaDto
                    {
                        Id = p.Id,
                        Numero = p.Numero,
                        Valor = p.Valor,
                        DataVencimento = p.DataVencimento,
                        DataPagamento = p.DataPagamento,
                        Status = p.Status
                    })
                    : Enumerable.Empty<ContaPagarParcelaDto>()
            });
        }
    }
}
