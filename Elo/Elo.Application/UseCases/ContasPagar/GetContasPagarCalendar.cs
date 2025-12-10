using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.ContasPagar;

public static class GetContasPagarCalendar
{
    public class Query : IRequest<IEnumerable<ContaPagarCalendarEventDto>>
    {
        public int? EmpresaId { get; set; }
        public ContaStatus? Status { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<ContaPagarCalendarEventDto>>
    {
        private readonly IContaPagarService _contaPagarService;
        private readonly IPessoaService _pessoaService;

        public Handler(IContaPagarService contaPagarService, IPessoaService pessoaService)
        {
            _contaPagarService = contaPagarService;
            _pessoaService = pessoaService;
        }

        public async Task<IEnumerable<ContaPagarCalendarEventDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var contas = await _contaPagarService.ObterContasPagarAsync(
                request.EmpresaId,
                null,
                request.Status,
                null,
                request.DataInicial,
                request.DataFinal);

            var lista = contas.ToList();
            if (!lista.Any()) return Enumerable.Empty<ContaPagarCalendarEventDto>();

            var fornecedoresIds = lista.Where(c => c.FornecedorId.HasValue).Select(c => c.FornecedorId!.Value).Distinct().ToList();
            var fornecedores = await _pessoaService.ObterPessoasPorIdsAsync(fornecedoresIds, request.EmpresaId ?? 0);
            var fornecedoresLookup = fornecedores.ToDictionary(f => f.Id, f => f.Nome);

            var grupos = lista
                .GroupBy(c => c.DataVencimento.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ContaPagarCalendarEventDto
                {
                    Data = g.Key,
                    ValorTotal = g.Sum(c => c.Valor),
                    Contas = g.OrderBy(c => c.DataVencimento).Select(c => new ContaPagarCalendarItemDto
                    {
                        Id = c.Id,
                        Descricao = c.Descricao,
                        Valor = c.Valor,
                        DataVencimento = c.DataVencimento,
                        Status = c.Status,
                        FornecedorId = c.FornecedorId,
                        FornecedorNome = (c.FornecedorId.HasValue && fornecedoresLookup.TryGetValue(c.FornecedorId.Value, out var nome)) ? nome : string.Empty
                    })
                });

            return grupos;
        }
    }
}
