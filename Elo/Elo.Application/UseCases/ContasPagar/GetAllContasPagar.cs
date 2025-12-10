using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.ContasPagar;

public static class GetAllContasPagar
{
    public class Query : IRequest<IEnumerable<ContaPagarDto>>
    {
        public int? EmpresaId { get; set; }
        public ContaStatus? Status { get; set; }
        public string? Categoria { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<ContaPagarDto>>
    {
        private readonly IContaPagarService _contaPagarService;
        private readonly IPessoaService _pessoaService;

        public Handler(IContaPagarService contaPagarService, IPessoaService pessoaService)
        {
            _contaPagarService = contaPagarService;
            _pessoaService = pessoaService;
        }

        public async Task<IEnumerable<ContaPagarDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var contas = await _contaPagarService.ObterContasPagarAsync(
                request.EmpresaId,
                null, 
                request.Status, 
                request.Categoria,
                request.DataInicial, 
                request.DataFinal);

            var lista = contas.ToList();
            if (!lista.Any()) return Enumerable.Empty<ContaPagarDto>();

            var contaIds = lista.Select(c => c.Id).Distinct().ToList();
            var fornecedoresIds = lista.Where(c => c.FornecedorId.HasValue).Select(c => c.FornecedorId!.Value).Distinct().ToList();

            var fornecedores = await _pessoaService.ObterPessoasPorIdsAsync(fornecedoresIds, request.EmpresaId ?? 0);
            var fornecedorLookup = fornecedores.ToDictionary(c => c.Id, c => c.Nome);

            var itens = await _contaPagarService.ObterItensPorListaIdsAsync(contaIds);
            var parcelas = await _contaPagarService.ObterParcelasPorListaIdsAsync(contaIds);

            var itensLookup = itens.GroupBy(i => i.ContaPagarId).ToDictionary(g => g.Key, g => g.ToList());
            var parcelasLookup = parcelas.GroupBy(p => p.ContaPagarId).ToDictionary(g => g.Key, g => g.OrderBy(pi => pi.Numero).ToList());

            return lista.OrderByDescending(c => c.CreatedAt).Select(c => new ContaPagarDto
            {
                Id = c.Id,
                EmpresaId = c.EmpresaId,
                FornecedorId = c.FornecedorId,
                AfiliadoId = c.AfiliadoId,
                FornecedorNome = (c.FornecedorId.HasValue && fornecedorLookup.TryGetValue(c.FornecedorId.Value, out var nome)) ? nome : string.Empty,
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
                Itens = itensLookup.TryGetValue(c.Id, out var contaItens)
                    ? contaItens.Select(i => new ContaPagarItemDto
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
