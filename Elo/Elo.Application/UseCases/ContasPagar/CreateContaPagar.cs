using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.ContasPagar;

public static class CreateContaPagar
{
    public class Command : IRequest<ContaPagarDto>
    {
        public int EmpresaId { get; set; }
        public CreateContaPagarDto Dto { get; set; } = new();
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

            // Preparar itens
            var itensInput = (dto.Itens ?? Enumerable.Empty<ContaFinanceiraItemInputDto>())
                .Where(i => i != null && i.Valor > 0 && (!string.IsNullOrWhiteSpace(i.Descricao) || i.ProdutoId.HasValue || i.ProdutoModuloId.HasValue || (i.ProdutoModuloIds?.Any() ?? false)))
                .Select(i => new ContaPagarItemInput(
                    i.Descricao,
                    i.Valor,
                    i.ProdutoId,
                    NormalizarModulos(i)
                ))
                .ToList();

            // Criar via service
            var conta = await _contaPagarService.CriarContaPagarAsync(
                dto.FornecedorId,
                dto.Descricao,
                dto.Valor,
                dto.DataVencimento,
                dto.DataPagamento,
                dto.Status,
                dto.Categoria,
                dto.IsRecorrente,
                dto.NumeroParcelas,
                dto.IntervaloDias,
                request.EmpresaId,
                itensInput,
                null,
                dto.AfiliadoId);

            // Fetch related data for DTO using Services
            string fornecedorNome = string.Empty;
            if (conta.FornecedorId.HasValue)
            {
                var fornecedor = await _pessoaService.ObterPessoaPorIdAsync(conta.FornecedorId.Value, Domain.Enums.PessoaTipo.Fornecedor, conta.EmpresaId);
                fornecedorNome = fornecedor?.Nome ?? string.Empty;
            }
            else if (conta.AfiliadoId.HasValue)
            {
                 // We don't have AfiliadoService injected here yet, maybe we should?
                 // Or just leave empty for now as DTO focuses on FornecedorNome.
                 // Ideally we should have AfiliadoNome in DTO or use FornecedorNome as generic.
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

        private static List<int> NormalizarModulos(ContaFinanceiraItemInputDto item)
        {
            var lista = item.ProdutoModuloIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            if (!lista.Any() && item.ProdutoModuloId.HasValue && item.ProdutoModuloId.Value > 0)
            {
                lista.Add(item.ProdutoModuloId.Value);
            }
            return lista;
        }
    }
}
