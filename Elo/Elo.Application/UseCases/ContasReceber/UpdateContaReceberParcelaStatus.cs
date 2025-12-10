using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

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
        private readonly IContaReceberService _contaReceberService;

        public Handler(IContaReceberService contaReceberService)
        {
            _contaReceberService = contaReceberService;
        }

        public async Task<ContaReceberParcelaDto> Handle(Command request, CancellationToken cancellationToken)
        {
            // O Service já trata a lógica de atualizar o status da parcela 
            // e atualizar a conta se todas as parcelas estiverem pagas?
            // Eu preciso garantir que o Service faça isso.
            // O IContaReceberService.AtualizarStatusParcelaAsync foi definido.
            // Preciso verificar se a implementação dele trata a conta pai.
            
            var parcela = await _contaReceberService.AtualizarStatusParcelaAsync(
                request.ParcelaId,
                request.Status,
                request.DataRecebimento,
                request.EmpresaId);

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
    }
}
