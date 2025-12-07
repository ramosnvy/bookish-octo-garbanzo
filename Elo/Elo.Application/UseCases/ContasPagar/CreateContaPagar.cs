using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPessoaService _pessoaService;

        public Handler(IUnitOfWork unitOfWork, IPessoaService pessoaService)
        {
            _unitOfWork = unitOfWork;
            _pessoaService = pessoaService;
        }

        public async Task<ContaPagarDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var fornecedor = await _pessoaService.ObterPessoaPorIdAsync(request.Dto.FornecedorId, PessoaTipo.Fornecedor, request.EmpresaId);
            if (fornecedor == null)
            {
                throw new KeyNotFoundException("Fornecedor não encontrado para esta empresa.");
            }

            var itens = (request.Dto.Itens ?? Enumerable.Empty<ContaFinanceiraItemInputDto>())
                .Where(i => i != null && i.Valor > 0 && (!string.IsNullOrWhiteSpace(i.Descricao) || i.ProdutoId.HasValue || i.ProdutoModuloId.HasValue || (i.ProdutoModuloIds?.Any() ?? false)))
                .ToList();

            var totalItens = itens.Sum(i => i.Valor);
            var valorTotal = totalItens > 0 ? totalItens : request.Dto.Valor;
            if (valorTotal <= 0)
            {
                throw new InvalidOperationException("Informe o valor total ou adicione itens com valores válidos.");
            }

            var numeroParcelas = request.Dto.NumeroParcelas.HasValue && request.Dto.NumeroParcelas.Value > 0 ? request.Dto.NumeroParcelas.Value : 1;
            var intervaloDias = request.Dto.IntervaloDias.HasValue && request.Dto.IntervaloDias.Value > 0 ? request.Dto.IntervaloDias.Value : 30;
            var dataVencimento = EnsureUtc(request.Dto.DataVencimento);
            var dataPagamento = EnsureUtcNullable(request.Dto.DataPagamento);

            var conta = new ContaPagar
            {
                EmpresaId = request.EmpresaId,
                FornecedorId = request.Dto.FornecedorId,
                Descricao = request.Dto.Descricao,
                Valor = valorTotal,
                DataVencimento = dataVencimento,
                DataPagamento = dataPagamento,
                Status = request.Dto.Status,
                Categoria = request.Dto.Categoria,
                IsRecorrente = request.Dto.IsRecorrente,
                TotalParcelas = numeroParcelas,
                IntervaloDias = intervaloDias,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ContasPagar.AddAsync(conta);
            await _unitOfWork.SaveChangesAsync();

            if (itens.Any())
            {
                foreach (var item in itens)
                {
                    var modulos = NormalizarModulos(item);
                    await _unitOfWork.ContaPagarItens.AddAsync(new ContaPagarItem
                    {
                        EmpresaId = request.EmpresaId,
                        ContaPagarId = conta.Id,
                        ProdutoId = item.ProdutoId,
                        ProdutoModuloIds = modulos,
                        Descricao = string.IsNullOrWhiteSpace(item.Descricao) ? conta.Descricao : item.Descricao,
                        Valor = item.Valor
                    });
                }
                await _unitOfWork.SaveChangesAsync();
            }

            var parcelas = GerarParcelas(conta, numeroParcelas, intervaloDias, valorTotal);
            foreach (var parcela in parcelas)
            {
                await _unitOfWork.ContaPagarParcelas.AddAsync(parcela);
            }
            if (parcelas.Any())
            {
                await _unitOfWork.SaveChangesAsync();
            }

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
                Itens = itens.Select(i =>
                {
                    var modulos = NormalizarModulos(i);
                    return new ContaPagarItemDto
                    {
                        Id = 0,
                        ContaPagarId = conta.Id,
                        Descricao = string.IsNullOrWhiteSpace(i.Descricao) ? conta.Descricao : i.Descricao,
                        Valor = i.Valor,
                        ProdutoId = i.ProdutoId,
                        ProdutoModuloId = modulos.FirstOrDefault(),
                        ProdutoModuloIds = modulos
                    };
                }),
                Parcelas = parcelas.Select(p => new ContaPagarParcelaDto
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

        private static List<ContaPagarParcela> GerarParcelas(ContaPagar conta, int numeroParcelas, int intervaloDias, decimal valorTotal)
        {
            var parcelas = new List<ContaPagarParcela>();
            var gerarValorRepetido = conta.IsRecorrente;
            var valorBase = gerarValorRepetido
                ? valorTotal
                : Math.Round(valorTotal / numeroParcelas, 2, MidpointRounding.AwayFromZero);
            decimal acumulado = 0;

            for (int i = 1; i <= numeroParcelas; i++)
            {
                var valor = gerarValorRepetido
                    ? valorTotal
                    : (i == numeroParcelas ? valorTotal - acumulado : valorBase);
                if (!gerarValorRepetido)
                {
                    acumulado += valor;
                }
                var vencimento = conta.DataVencimento.AddDays(intervaloDias * (i - 1));

                parcelas.Add(new ContaPagarParcela
                {
                    EmpresaId = conta.EmpresaId,
                    ContaPagarId = conta.Id,
                    Numero = i,
                    Valor = valor,
                    DataVencimento = vencimento,
                    Status = ContaStatus.Pendente
                });
            }

            return parcelas;
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
