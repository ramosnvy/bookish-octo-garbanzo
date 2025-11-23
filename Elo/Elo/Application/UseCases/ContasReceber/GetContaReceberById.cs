using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Interfaces.Repositories;

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
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ContaReceberDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var conta = await _unitOfWork.ContasReceber.GetByIdAsync(request.Id);
            if (conta == null)
            {
                return null;
            }

            if (request.EmpresaId.HasValue && conta.EmpresaId != request.EmpresaId.Value)
            {
                return null;
            }

            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(conta.ClienteId);
            var itens = await _unitOfWork.ContaReceberItens.FindAsync(i => i.ContaReceberId == conta.Id);
            var parcelas = await _unitOfWork.ContaReceberParcelas.FindAsync(p => p.ContaReceberId == conta.Id);

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
                IsRecorrente = conta.IsRecorrente,
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
