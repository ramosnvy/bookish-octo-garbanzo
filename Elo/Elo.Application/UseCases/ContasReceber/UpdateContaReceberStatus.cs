using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.ContasReceber;

public static class UpdateContaReceberStatus
{
    public class Command : IRequest<ContaReceberDto>
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public ContaStatus Status { get; set; }
        public DateTime? DataRecebimento { get; set; }
    }

    public class Handler : IRequestHandler<Command, ContaReceberDto>
    {
        private readonly IContaReceberService _contaReceberService;
        private readonly IPessoaService _pessoaService;

        public Handler(IContaReceberService contaReceberService, IPessoaService pessoaService)
        {
            _contaReceberService = contaReceberService;
            _pessoaService = pessoaService;
        }

        public async Task<ContaReceberDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var original = await _contaReceberService.ObterContaReceberPorIdAsync(request.Id, request.EmpresaId);
            if (original == null) throw new KeyNotFoundException("Conta não encontrada.");

            var conta = await _contaReceberService.AtualizarContaReceberAsync(
                request.Id,
                original.ClienteId,
                original.Descricao,
                original.Valor,
                original.DataVencimento,
                request.DataRecebimento,
                request.Status,
                original.FormaPagamento,
                request.EmpresaId);

            var cliente = await _pessoaService.ObterPessoaPorIdAsync(conta.ClienteId, PessoaTipo.Cliente, request.EmpresaId);
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
