using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.ContasPagar;

public static class GetContaPagarById
{
    public class Query : IRequest<ContaPagarDto?>
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, ContaPagarDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ContaPagarDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var conta = await _unitOfWork.ContasPagar.GetByIdAsync(request.Id);
            if (conta == null)
            {
                return null;
            }

            if (request.EmpresaId.HasValue && conta.EmpresaId != request.EmpresaId.Value)
            {
                return null;
            }

            var fornecedor = await _unitOfWork.Pessoas.GetByIdAsync(conta.FornecedorId);
            var itens = await _unitOfWork.ContaPagarItens.FindAsync(i => i.ContaPagarId == conta.Id);
            var parcelas = await _unitOfWork.ContaPagarParcelas.FindAsync(p => p.ContaPagarId == conta.Id);

            return new ContaPagarDto
            {
                Id = conta.Id,
                EmpresaId = conta.EmpresaId,
                FornecedorId = conta.FornecedorId,
                FornecedorNome = fornecedor?.Nome ?? string.Empty,
                Descricao = conta.Descricao,
                Valor = conta.Valor,
                DataVencimento = conta.DataVencimento,
                DataPagamento = conta.DataPagamento,
                Status = conta.Status,
                Categoria = conta.Categoria,
                IsRecorrente = conta.IsRecorrente,
                TotalParcelas = conta.TotalParcelas,
                IntervaloDias = conta.IntervaloDias,
                CreatedAt = conta.CreatedAt,
                UpdatedAt = conta.UpdatedAt,
                Itens = itens.Select(i => new ContaPagarItemDto
                {
                    Id = i.Id,
                    ContaPagarId = i.ContaPagarId,
                    Descricao = i.Descricao,
                    Valor = i.Valor,
                    ProdutoId = i.ProdutoId,
                    ProdutoModuloId = i.ProdutoModuloIds?.FirstOrDefault(),
                    ProdutoModuloIds = i.ProdutoModuloIds ?? new List<int>()
                }),
                Parcelas = parcelas.OrderBy(p => p.Numero).Select(p => new ContaPagarParcelaDto
                {
                    Id = p.Id,
                    Numero = p.Numero,
                    Valor = p.Valor,
                    DataVencimento = p.DataVencimento,
                    DataPagamento = p.DataPagamento,
                    Status = p.Status
                })
            };
        }
    }
}
