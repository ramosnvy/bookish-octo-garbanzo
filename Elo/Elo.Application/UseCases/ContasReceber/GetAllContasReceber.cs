using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.ContasReceber;

public static class GetAllContasReceber
{
    public class Query : IRequest<IEnumerable<ContaReceberDto>>
    {
        public int? EmpresaId { get; set; }
        public ContaStatus? Status { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<ContaReceberDto>>
    {
        private readonly IContaReceberService _contaReceberService;
        private readonly IPessoaService _pessoaService;

        public Handler(IContaReceberService contaReceberService, IPessoaService pessoaService)
        {
            _contaReceberService = contaReceberService;
            _pessoaService = pessoaService;
        }

        public async Task<IEnumerable<ContaReceberDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var contas = await _contaReceberService.ObterContasReceberAsync(
                request.EmpresaId,
                null, 
                request.Status, 
                request.DataInicial, 
                request.DataFinal);

            var lista = contas.ToList();
            if (!lista.Any()) return Enumerable.Empty<ContaReceberDto>();

            var contaIds = lista.Select(c => c.Id).Distinct().ToList();
            var clientesIds = lista.Select(c => c.ClienteId).Distinct().ToList();

            var clientes = await _pessoaService.ObterPessoasPorIdsAsync(clientesIds, request.EmpresaId); 
            var clienteLookup = clientes.ToDictionary(c => c.Id, c => c.Nome);

            var itens = await _contaReceberService.ObterItensPorListaIdsAsync(contaIds);
            var parcelas = await _contaReceberService.ObterParcelasPorListaIdsAsync(contaIds);

            var itensLookup = itens.GroupBy(i => i.ContaReceberId).ToDictionary(g => g.Key, g => g.ToList());
            var parcelasLookup = parcelas.GroupBy(p => p.ContaReceberId).ToDictionary(g => g.Key, g => g.OrderBy(pi => pi.Numero).ToList());

            return lista.OrderByDescending(c => c.CreatedAt).Select(c => new ContaReceberDto
            {
                Id = c.Id,
                EmpresaId = c.EmpresaId,
                ClienteId = c.ClienteId,
                ClienteNome = clienteLookup.TryGetValue(c.ClienteId, out var nome) ? nome : string.Empty,
                Descricao = c.Descricao,
                Valor = c.Valor,
                DataVencimento = c.DataVencimento,
                DataRecebimento = c.DataRecebimento,
                Status = c.Status,
                FormaPagamento = c.FormaPagamento,
                IsRecorrente = false,
                TotalParcelas = c.TotalParcelas,
                IntervaloDias = c.IntervaloDias,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Itens = itensLookup.TryGetValue(c.Id, out var contaItens)
                    ? contaItens.Select(i => new ContaReceberItemDto
                    {
                        Id = i.Id,
                        ContaReceberId = i.ContaReceberId,
                        Descricao = i.Descricao,
                        Valor = i.Valor,
                        ProdutoId = i.ProdutoId,
                        ProdutoModuloId = i.ProdutoModuloIds?.FirstOrDefault(),
                        ProdutoModuloIds = i.ProdutoModuloIds ?? new List<int>()
                    })
                    : Enumerable.Empty<ContaReceberItemDto>(),
                Parcelas = parcelasLookup.TryGetValue(c.Id, out var contaParcelas)
                    ? contaParcelas.Select(p => new ContaReceberParcelaDto
                    {
                        Id = p.Id,
                        Numero = p.Numero,
                        Valor = p.Valor,
                        DataVencimento = p.DataVencimento,
                        DataRecebimento = p.DataRecebimento,
                        Status = p.Status
                    })
                    : Enumerable.Empty<ContaReceberParcelaDto>()
            });
        }
    }
}
