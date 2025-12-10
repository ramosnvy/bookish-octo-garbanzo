using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

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
        private readonly IContaPagarService _contaPagarService;
        private readonly IPessoaService _pessoaService;

        public Handler(IContaPagarService contaPagarService, IPessoaService pessoaService)
        {
            _contaPagarService = contaPagarService;
            _pessoaService = pessoaService;
        }

        public async Task<ContaPagarDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            // Optional: Duplicate validations if needed, but Service should check existence.
            // But checking items/parcelas changes "InvalidOperationException" should be in Service or here?
            // "Esta conta já possui itens/parcelas definidas. Crie um novo plano..."
            // This is application validation. We can check if items/parcels are present in DTO.
            
            if ((dto.Itens?.Any() ?? false) || dto.NumeroParcelas.HasValue)
            {
                 // Check if original has items? No, logic was "If you try to set/change items in UpdateDto, throw".
                 // Assuming Dto logic.
                 throw new InvalidOperationException("Esta conta já possui itens/parcelas definidas. Crie um novo plano para alterar composição.");
            }

            // We need to fetch original to check if Value changed, in original handler.
            var original = await _contaPagarService.ObterContaPagarPorIdAsync(dto.Id, request.EmpresaId);
            if (original == null) throw new KeyNotFoundException("Conta não encontrada.");

            if (dto.Valor != original.Valor)
            {
                throw new InvalidOperationException("Valor total não pode ser alterado após gerar parcelas.");
            }

            var conta = await _contaPagarService.AtualizarContaPagarAsync(
                dto.Id,
                dto.FornecedorId,
                dto.Descricao,
                dto.Valor,
                dto.DataVencimento,
                dto.DataPagamento,
                dto.Status,
                dto.Categoria,
                dto.IsRecorrente,
                request.EmpresaId,
                dto.AfiliadoId);

            string fornecedorNome = string.Empty;
            if (conta.FornecedorId.HasValue)
            {
                var fornecedor = await _pessoaService.ObterPessoaPorIdAsync(conta.FornecedorId.Value, PessoaTipo.Fornecedor, request.EmpresaId);
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
