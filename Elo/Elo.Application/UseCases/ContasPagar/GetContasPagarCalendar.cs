using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

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
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ContaPagarCalendarEventDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var contas = request.EmpresaId.HasValue
                ? await _unitOfWork.ContasPagar.FindAsync(c => c.EmpresaId == request.EmpresaId.Value)
                : await _unitOfWork.ContasPagar.GetAllAsync();

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
                : Enumerable.Empty<Pessoa>();
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
                        FornecedorNome = fornecedoresLookup.TryGetValue(c.FornecedorId, out var nome) ? nome : string.Empty
                    })
                });

            return grupos;
        }
    }
}
