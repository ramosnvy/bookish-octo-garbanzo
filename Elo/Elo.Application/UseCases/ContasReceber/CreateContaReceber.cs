using MediatR;
using Elo.Application.DTOs.Financeiro;
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

            // Processar itens
            var itens = (dto.Itens ?? Enumerable.Empty<ContaFinanceiraItemInputDto>())
                .Where(i => i != null && i.Valor > 0 && !string.IsNullOrWhiteSpace(i.Descricao))
                .Select(i => new ContaReceberItemInput(i.Descricao, i.Valor))
                .ToList();

            // Criar conta via service
            var conta = await _contaReceberService.CriarContaReceberAsync(
                dto.ClienteId,
                dto.Descricao,
                dto.Valor,
                dto.DataVencimento,
                dto.DataRecebimento,
                dto.Status,
                dto.FormaPagamento,
                dto.NumeroParcelas,
                dto.IntervaloDias,
                request.EmpresaId,
                itens);

            // Buscar dados relacionados para o DTO
            var cliente = await _pessoaService.ObterPessoaPorIdAsync(
                conta.ClienteId,
                Domain.Enums.PessoaTipo.Cliente,
                request.EmpresaId);

            var contaItens = await _unitOfWork.ContaReceberItens.FindAsync(i => i.ContaReceberId == conta.Id);
            var parcelas = await _unitOfWork.ContaReceberParcelas.FindAsync(p => p.ContaReceberId == conta.Id);

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
                Itens = contaItens.Select(i => new ContaReceberItemDto
                {
                    Id = i.Id,
                    ContaReceberId = i.ContaReceberId,
                    Descricao = i.Descricao,
                    Valor = i.Valor,
                    ProdutoId = i.ProdutoId,
                    ProdutoModuloId = null,
                    ProdutoModuloIds = i.ProdutoModuloIds
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
    }
}
