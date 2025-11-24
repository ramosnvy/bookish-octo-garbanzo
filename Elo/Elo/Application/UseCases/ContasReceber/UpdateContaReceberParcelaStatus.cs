using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.ContasReceber;

public static class UpdateContaReceberParcelaStatus
{
    public class Command : IRequest<ContaReceberParcelaDto>
    {
        public int EmpresaId { get; set; }
        public int ContaId { get; set; }
        public int ParcelaId { get; set; }
        public ContaStatus Status { get; set; }
        public DateTime? DataRecebimento { get; set; }
    }

    public class Handler : IRequestHandler<Command, ContaReceberParcelaDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ContaReceberParcelaDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var conta = await _unitOfWork.ContasReceber.GetByIdAsync(request.ContaId)
                ?? throw new KeyNotFoundException("Conta não encontrada.");
            if (conta.EmpresaId != request.EmpresaId)
            {
                throw new UnauthorizedAccessException("Conta pertence a outra empresa.");
            }

            var parcela = await _unitOfWork.ContaReceberParcelas.GetByIdAsync(request.ParcelaId)
                ?? throw new KeyNotFoundException("Parcela não encontrada.");
            if (parcela.ContaReceberId != conta.Id)
            {
                throw new InvalidOperationException("Parcela não pertence à conta informada.");
            }

            parcela.Status = request.Status;
            parcela.DataRecebimento = request.Status == ContaStatus.Pago
                ? EnsureUtcNullable(request.DataRecebimento) ?? DateTime.UtcNow
                : null;
            parcela.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ContaReceberParcelas.UpdateAsync(parcela);

            // Atualiza o status da conta quando todas as parcelas estiverem pagas.
            if (request.Status == ContaStatus.Pago)
            {
                var parcelasConta = await _unitOfWork.ContaReceberParcelas.FindAsync(p => p.ContaReceberId == conta.Id);
                if (parcelasConta.All(p => p.Status == ContaStatus.Pago))
                {
                    conta.Status = ContaStatus.Pago;
                    conta.DataRecebimento = parcela.DataRecebimento;
                    conta.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.ContasReceber.UpdateAsync(conta);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return new ContaReceberParcelaDto
            {
                Id = parcela.Id,
                Numero = parcela.Numero,
                Valor = parcela.Valor,
                DataVencimento = parcela.DataVencimento,
                DataRecebimento = parcela.DataRecebimento,
                Status = parcela.Status
            };
        }

        private static DateTime? EnsureUtcNullable(DateTime? value)
        {
            return value?.ToUniversalTime();
        }
    }
}
