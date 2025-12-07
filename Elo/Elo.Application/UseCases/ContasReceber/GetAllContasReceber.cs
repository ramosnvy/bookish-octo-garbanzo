using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

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
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ContaReceberDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            IEnumerable<Domain.Entities.ContaReceber> contas = request.EmpresaId.HasValue
                ? await _unitOfWork.ContasReceber.FindAsync(c => c.EmpresaId == request.EmpresaId.Value)
                : await _unitOfWork.ContasReceber.GetAllAsync();

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

            var clientesIds = lista.Select(c => c.ClienteId).Distinct().ToList();
            var clientes = clientesIds.Any()
                ? await _unitOfWork.Pessoas.FindAsync(p => clientesIds.Contains(p.Id))
                : Enumerable.Empty<Domain.Entities.Pessoa>();
            var clienteLookup = clientes.ToDictionary(c => c.Id, c => c.Nome);

            var contaIds = lista.Select(c => c.Id).ToList();
            var itens = contaIds.Any()
                ? await _unitOfWork.ContaReceberItens.FindAsync(i => contaIds.Contains(i.ContaReceberId))
                : Enumerable.Empty<Domain.Entities.ContaReceberItem>();
            var parcelas = contaIds.Any()
                ? await _unitOfWork.ContaReceberParcelas.FindAsync(p => contaIds.Contains(p.ContaReceberId))
                : Enumerable.Empty<Domain.Entities.ContaReceberParcela>();
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
