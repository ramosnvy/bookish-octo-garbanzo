using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.ContasReceber;

public static class UpdateContaReceber
{
    public class Command : IRequest<ContaReceberDto>
    {
        public int EmpresaId { get; set; }
        public UpdateContaReceberDto Dto { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, ContaReceberDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPessoaService _pessoaService;

        public Handler(IUnitOfWork unitOfWork, IPessoaService pessoaService)
        {
            _unitOfWork = unitOfWork;
            _pessoaService = pessoaService;
        }

        public async Task<ContaReceberDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var conta = await _unitOfWork.ContasReceber.GetByIdAsync(request.Dto.Id) ?? throw new KeyNotFoundException("Conta não encontrada.");
            if (conta.EmpresaId != request.EmpresaId)
            {
                throw new UnauthorizedAccessException("Conta pertence a outra empresa.");
            }

            if ((request.Dto.Itens?.Any() ?? false) || request.Dto.NumeroParcelas.HasValue)
            {
                throw new InvalidOperationException("Esta conta já possui parcelas definidas. Para alterar itens, gere uma nova cobrança.");
            }

            if (request.Dto.Valor != conta.Valor)
            {
                throw new InvalidOperationException("Valor total não pode ser alterado após gerar parcelas.");
            }

            var cliente = await _pessoaService.ObterPessoaPorIdAsync(request.Dto.ClienteId, PessoaTipo.Cliente, request.EmpresaId);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente não encontrado para esta empresa.");
            }

            var normalizedDataRecebimento = EnsureUtcNullable(request.Dto.DataRecebimento);

            conta.ClienteId = request.Dto.ClienteId;
            conta.Descricao = request.Dto.Descricao;
            conta.Valor = request.Dto.Valor;
            conta.DataVencimento = EnsureUtc(request.Dto.DataVencimento);
            conta.DataRecebimento = normalizedDataRecebimento;
            conta.Status = request.Dto.Status;
            conta.FormaPagamento = request.Dto.FormaPagamento;
            conta.IsRecorrente = false;
            conta.UpdatedAt = DateTime.UtcNow;

            var parcelas = (await _unitOfWork.ContaReceberParcelas.FindAsync(p => p.ContaReceberId == conta.Id)).ToList();

            await _unitOfWork.ContasReceber.UpdateAsync(conta);

            if (parcelas.Any())
            {
                foreach (var parcela in parcelas)
                {
                    parcela.Status = conta.Status;
                    parcela.DataRecebimento = conta.Status == ContaStatus.Pago
                        ? normalizedDataRecebimento ?? parcela.DataRecebimento
                        : null;
                    await _unitOfWork.ContaReceberParcelas.UpdateAsync(parcela);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var itens = await _unitOfWork.ContaReceberItens.FindAsync(i => i.ContaReceberId == conta.Id);

            return new ContaReceberDto
            {
                Id = conta.Id,
                EmpresaId = conta.EmpresaId,
                ClienteId = conta.ClienteId,
                ClienteNome = cliente.Nome,
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
