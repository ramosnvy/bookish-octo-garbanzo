using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.ContasReceber;

public static class GetContaReceberById
{
    public class Query : IRequest<ContaReceberDto?>
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, ContaReceberDto?>
    {
        private readonly IContaReceberService _contaReceberService;
        private readonly IPessoaService _pessoaService;

        public Handler(IContaReceberService contaReceberService, IPessoaService pessoaService)
        {
            _contaReceberService = contaReceberService;
            _pessoaService = pessoaService;
        }

        public async Task<ContaReceberDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var conta = await _contaReceberService.ObterContaReceberPorIdAsync(request.Id, request.EmpresaId ?? 0);
            if (conta == null)
            {
                return null;
            }

            var cliente = await _pessoaService.ObterPessoaPorIdAsync(
                conta.ClienteId,
                PessoaTipo.Cliente,
                conta.EmpresaId);

            var itens = await _contaReceberService.ObterItensPorContaIdAsync(conta.Id);
            var parcelas = await _contaReceberService.ObterParcelasPorContaIdAsync(conta.Id);

            return new ContaReceberDto
            {
                Id = conta.Id,
                EmpresaId = conta.EmpresaId,
                ClienteId = conta.ClienteId,
                ClienteNome = cliente?.Nome ?? string.Empty,
                Descricao = conta.Descricao,
                Valor = conta.Valor,
                DataVencimento = conta.DataVencimento,
                DataRecebimento = conta.DataRecebimento,
                Status = conta.Status,
                FormaPagamento = conta.FormaPagamento,
                IsRecorrente = false,
                TotalParcelas = conta.TotalParcelas,
                IntervaloDias = conta.IntervaloDias,
                CreatedAt = conta.CreatedAt,
                UpdatedAt = conta.UpdatedAt,
                Itens = itens.Select(i => new ContaReceberItemDto
                {
                    Id = i.Id,
                    ContaReceberId = i.ContaReceberId,
                    Descricao = i.Descricao,
                    Valor = i.Valor,
                    ProdutoId = i.ProdutoId,
                    ProdutoModuloId = i.ProdutoModuloIds?.FirstOrDefault(),
                    ProdutoModuloIds = i.ProdutoModuloIds ?? new List<int>()
                }),
                Parcelas = parcelas.OrderBy(p => p.Numero).Select(p => new ContaReceberParcelaDto
                {
                    Id = p.Id,
                    Numero = p.Numero,
                    Valor = p.Valor,
                    DataVencimento = p.DataVencimento,
                    DataRecebimento = p.DataRecebimento,
                    Status = p.Status
                })
            };
        }
    }
}
