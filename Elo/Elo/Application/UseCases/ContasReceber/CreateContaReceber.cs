using MediatR;
using Elo.Application.DTOs.Financeiro;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.ContasReceber;

public static class CreateContaReceber
{
    public class Command : IRequest<ContaReceberDto>
    {
        public int EmpresaId { get; set; }
        public CreateContaReceberDto Dto { get; set; } = new();
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
            var cliente = await _pessoaService.ObterPessoaPorIdAsync(request.Dto.ClienteId, PessoaTipo.Cliente, request.EmpresaId);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente não encontrado para esta empresa.");
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
            var dataRecebimento = EnsureUtcNullable(request.Dto.DataRecebimento);

            var conta = new ContaReceber
            {
                EmpresaId = request.EmpresaId,
                ClienteId = request.Dto.ClienteId,
                Descricao = request.Dto.Descricao,
                Valor = valorTotal,
                DataVencimento = dataVencimento,
                DataRecebimento = dataRecebimento,
                Status = request.Dto.Status,
                FormaPagamento = request.Dto.FormaPagamento,
                IsRecorrente = request.Dto.IsRecorrente,
                TotalParcelas = numeroParcelas,
                IntervaloDias = intervaloDias,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ContasReceber.AddAsync(conta);
            await _unitOfWork.SaveChangesAsync();

            if (itens.Any())
            {
                foreach (var item in itens)
                {
                    var modulos = NormalizarModulos(item);
                    await _unitOfWork.ContaReceberItens.AddAsync(new ContaReceberItem
                    {
                        EmpresaId = request.EmpresaId,
                        ContaReceberId = conta.Id,
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
                await _unitOfWork.ContaReceberParcelas.AddAsync(parcela);
            }
            if (parcelas.Any())
            {
                await _unitOfWork.SaveChangesAsync();
            }

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
                IsRecorrente = conta.IsRecorrente,
                TotalParcelas = conta.TotalParcelas,
                IntervaloDias = conta.IntervaloDias,
                CreatedAt = conta.CreatedAt,
                UpdatedAt = conta.UpdatedAt,
                Itens = itens.Select(i =>
                {
                    var modulos = NormalizarModulos(i);
                    return new ContaReceberItemDto
                    {
                        Id = 0,
                        ContaReceberId = conta.Id,
                        Descricao = string.IsNullOrWhiteSpace(i.Descricao) ? conta.Descricao : i.Descricao,
                        Valor = i.Valor,
                        ProdutoId = i.ProdutoId,
                        ProdutoModuloId = modulos.FirstOrDefault(),
                        ProdutoModuloIds = modulos
                    };
                }),
                Parcelas = parcelas.Select(p => new ContaReceberParcelaDto
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

        private static List<int> NormalizarModulos(ContaFinanceiraItemInputDto item)
        {
            var lista = item.ProdutoModuloIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            if (!lista.Any() && item.ProdutoModuloId.HasValue && item.ProdutoModuloId.Value > 0)
            {
                lista.Add(item.ProdutoModuloId.Value);
            }
            return lista;
        }

        private static List<ContaReceberParcela> GerarParcelas(ContaReceber conta, int numeroParcelas, int intervaloDias, decimal valorTotal)
        {
            var parcelas = new List<ContaReceberParcela>();
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

                parcelas.Add(new ContaReceberParcela
                {
                    EmpresaId = conta.EmpresaId,
                    ContaReceberId = conta.Id,
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
