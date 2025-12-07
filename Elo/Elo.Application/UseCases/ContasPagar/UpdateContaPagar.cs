using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;
using Elo.Domain.Entities;

namespace Elo.Application.UseCases.ContasPagar;

public static class UpdateContaPagar
{
    public class Command : IRequest<ContaPagarDto>
    {
        public int EmpresaId { get; set; }
        public UpdateContaPagarDto Dto { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, ContaPagarDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPessoaService _pessoaService;

        public Handler(IUnitOfWork unitOfWork, IPessoaService pessoaService)
        {
            _unitOfWork = unitOfWork;
            _pessoaService = pessoaService;
        }

        public async Task<ContaPagarDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var conta = await _unitOfWork.ContasPagar.GetByIdAsync(request.Dto.Id) ?? throw new KeyNotFoundException("Conta não encontrada.");
            if (conta.EmpresaId != request.EmpresaId)
            {
                throw new UnauthorizedAccessException("Conta pertence a outra empresa.");
            }

            if ((request.Dto.Itens?.Any() ?? false) || request.Dto.NumeroParcelas.HasValue)
            {
                throw new InvalidOperationException("Esta conta já possui itens/parcelas definidas. Crie um novo plano para alterar composição.");
            }

            if (request.Dto.Valor != conta.Valor)
            {
                throw new InvalidOperationException("Valor total não pode ser alterado após gerar parcelas.");
            }

            var fornecedor = await _pessoaService.ObterPessoaPorIdAsync(request.Dto.FornecedorId, PessoaTipo.Fornecedor, request.EmpresaId);
            if (fornecedor == null)
            {
                throw new KeyNotFoundException("Fornecedor não encontrado para esta empresa.");
            }

            conta.FornecedorId = request.Dto.FornecedorId;
            conta.Descricao = request.Dto.Descricao;
            conta.Valor = request.Dto.Valor;
            conta.DataVencimento = EnsureUtc(request.Dto.DataVencimento);
            conta.DataPagamento = EnsureUtcNullable(request.Dto.DataPagamento);
            conta.Status = request.Dto.Status;
            conta.Categoria = request.Dto.Categoria;
            conta.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ContasPagar.UpdateAsync(conta);
            await _unitOfWork.SaveChangesAsync();

            var itens = await _unitOfWork.ContaPagarItens.FindAsync(i => i.ContaPagarId == conta.Id);
            var parcelas = await _unitOfWork.ContaPagarParcelas.FindAsync(p => p.ContaPagarId == conta.Id);

            return new ContaPagarDto
            {
                Id = conta.Id,
                EmpresaId = conta.EmpresaId,
                FornecedorId = conta.FornecedorId,
                FornecedorNome = fornecedor.Nome,
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
        private static DateTime EnsureUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private static DateTime? EnsureUtcNullable(DateTime? value)
        {
            return value.HasValue ? EnsureUtc(value.Value) : null;
        }
    }
}
