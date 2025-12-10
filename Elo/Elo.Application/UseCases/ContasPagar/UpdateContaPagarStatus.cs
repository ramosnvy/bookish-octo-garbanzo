using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.ContasPagar;

public static class UpdateContaPagarStatus
{
    public class Command : IRequest<ContaPagarDto>
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public ContaStatus Status { get; set; }
        public DateTime? DataPagamento { get; set; }
    }

    public class Handler : IRequestHandler<Command, ContaPagarDto>
    {
        private readonly IContaPagarService _contaPagarService;
        private readonly IPessoaService _pessoaService;

        public Handler(IContaPagarService contaPagarService, IPessoaService pessoaService)
        {
            _contaPagarService = contaPagarService;
            _pessoaService = pessoaService;
        }

        public async Task<ContaPagarDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var original = await _contaPagarService.ObterContaPagarPorIdAsync(request.Id, request.EmpresaId);
            if (original == null) throw new KeyNotFoundException("Conta não encontrada.");

            var conta = await _contaPagarService.AtualizarContaPagarAsync(
                request.Id,
                original.FornecedorId,
                original.Descricao,
                original.Valor,
                original.DataVencimento,
                request.DataPagamento,
                request.Status,
                original.Categoria,
                original.IsRecorrente,
                request.EmpresaId,
                original.AfiliadoId);

            string fornecedorNome = string.Empty;
            if (conta.FornecedorId.HasValue)
            {
                var fornecedor = await _pessoaService.ObterPessoaPorIdAsync(conta.FornecedorId.Value, PessoaTipo.Fornecedor, conta.EmpresaId);
                fornecedorNome = fornecedor?.Nome ?? string.Empty;
            }
            var itens = await _contaPagarService.ObterItensPorContaIdAsync(conta.Id);
            var parcelas = await _contaPagarService.ObterParcelasPorContaIdAsync(conta.Id);

            return new ContaPagarDto
            {
                Id = conta.Id,
                EmpresaId = conta.EmpresaId,
                FornecedorId = conta.FornecedorId,
                AfiliadoId = conta.AfiliadoId,
                FornecedorNome = fornecedorNome,
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
