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
        private readonly IContaReceberService _contaReceberService;
        private readonly IPessoaService _pessoaService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IContaReceberService contaReceberService,
            IPessoaService pessoaService,
            IUnitOfWork unitOfWork)
        {
            _contaReceberService = contaReceberService;
            _pessoaService = pessoaService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ContaReceberDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            // Validações de negócio específicas do update
            var contaExistente = await _contaReceberService.ObterContaReceberPorIdAsync(dto.Id, request.EmpresaId);
            if (contaExistente == null)
                throw new KeyNotFoundException("Conta não encontrada.");

            if ((dto.Itens?.Any() ?? false) || dto.NumeroParcelas.HasValue)
            {
                throw new InvalidOperationException("Esta conta já possui parcelas definidas. Para alterar itens, gere uma nova cobrança.");
            }

            if (dto.Valor != contaExistente.Valor)
            {
                throw new InvalidOperationException("Valor total não pode ser alterado após gerar parcelas.");
            }

            // Atualizar via service
            var conta = await _contaReceberService.AtualizarContaReceberAsync(
                dto.Id,
                dto.ClienteId,
                dto.Descricao,
                dto.Valor,
                dto.DataVencimento,
                dto.DataRecebimento,
                dto.Status,
                dto.FormaPagamento,
                request.EmpresaId);

            // Atualizar parcelas se necessário
            var parcelas = (await _unitOfWork.ContaReceberParcelas.FindAsync(p => p.ContaReceberId == conta.Id)).ToList();
            var normalizedDataRecebimento = EnsureUtcNullable(dto.DataRecebimento);

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
                await _unitOfWork.SaveChangesAsync();
            }

            // Buscar dados para o DTO
            var cliente = await _pessoaService.ObterPessoaPorIdAsync(conta.ClienteId, PessoaTipo.Cliente, request.EmpresaId);
            var itens = await _unitOfWork.ContaReceberItens.FindAsync(i => i.ContaReceberId == conta.Id);

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

        private static DateTime? EnsureUtcNullable(DateTime? value)
        {
            if (!value.HasValue) return null;
            
            return value.Value.Kind switch
            {
                DateTimeKind.Utc => value.Value,
                DateTimeKind.Local => value.Value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            };
        }
    }
}
