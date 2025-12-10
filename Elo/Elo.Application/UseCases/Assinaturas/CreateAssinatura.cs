using Elo.Application.DTOs.Assinatura;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;
using Elo.Domain.Services;
using Elo.Domain.Enums;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace Elo.Application.UseCases.Assinaturas;

public static class CreateAssinatura
{
    public record Command : IRequest<AssinaturaDto>
    {
        public int ClienteId { get; set; }
        public bool IsRecorrente { get; set; }
        public int? IntervaloDias { get; set; }
        public int? RecorrenciaQtde { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool GerarFinanceiro { get; set; }
        public bool GerarImplantacao { get; set; }
        [JsonPropertyName("formaPagamentoId")]
        public int? EmpresaFormaPagamentoId { get; set; }
        public int? AfiliadoId { get; set; }
        public List<AssinaturaItemRequest> Itens { get; set; } = new();
        [JsonIgnore]
        public int EmpresaId { get; set; }
        [JsonIgnore]
        public int? UsuarioId { get; set; }
    }

    public record AssinaturaItemRequest(int ProdutoId, int? ProdutoModuloId);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ClienteId).GreaterThan(0).WithMessage("Cliente inválido.");
            RuleFor(x => x.DataInicio).NotEmpty();
            RuleFor(x => x.Itens).NotEmpty().WithMessage("A assinatura deve ter pelo menos um item.");
            When(x => x.IsRecorrente, () => 
            {
                RuleFor(x => x.IntervaloDias).GreaterThan(0).WithMessage("Intervalo de dias deve ser maior que zero para assinaturas recorrentes.");
            });

            When(x => x.GerarFinanceiro, () => 
            {
                RuleFor(x => x.EmpresaFormaPagamentoId)
                    .NotNull().WithMessage("A forma de pagamento é obrigatória quando o financeiro é gerado.")
                    .GreaterThan(0).WithMessage("Forma de pagamento inválida.");
            });
        }
    }

    public class Handler : IRequestHandler<Command, AssinaturaDto>
    {
        private readonly IAssinaturaService _assinaturaService;
        private readonly IPessoaService _pessoaService;
        private readonly IProdutoService _produtoService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IAssinaturaService assinaturaService,
            IPessoaService pessoaService,
            IProdutoService produtoService,
            IUnitOfWork unitOfWork)
        {
            _assinaturaService = assinaturaService;
            _pessoaService = pessoaService;
            _produtoService = produtoService;
            _unitOfWork = unitOfWork;
        }

        public async Task<AssinaturaDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var itensInput = request.Itens.Select(i => new AssinaturaItemInput(i.ProdutoId, i.ProdutoModuloId)).ToList();
            
            FormaPagamento? formaPagamentoEnum = null;
            if (request.EmpresaFormaPagamentoId.HasValue)
            {
                var config = await _unitOfWork.EmpresaFormasPagamento.GetByIdAsync(request.EmpresaFormaPagamentoId.Value);
                if (config == null || config.EmpresaId != request.EmpresaId || !config.Ativo)
                {
                    throw new InvalidOperationException("Forma de pagamento não encontrada ou inativa.");
                }
                formaPagamentoEnum = config.FormaPagamento;
            }

            var assinatura = await _assinaturaService.CriarAssinaturaAsync(
                empresaId: request.EmpresaId,
                clienteId: request.ClienteId,
                isRecorrente: request.IsRecorrente,
                intervaloDias: request.IntervaloDias,
                recorrenciaQtde: request.RecorrenciaQtde,
                dataInicio: request.DataInicio,
                dataFim: request.DataFim,
                gerarFinanceiro: request.GerarFinanceiro,
                gerarImplantacao: request.GerarImplantacao,
                itens: itensInput,
                formaPagamento: formaPagamentoEnum,
                afiliadoId: request.AfiliadoId,
                usuarioCriadorId: request.UsuarioId
            );

            // Fetch details for DTO
            var cliente = await _pessoaService.ObterPessoaPorIdAsync(assinatura.ClienteId, Domain.Enums.PessoaTipo.Cliente, request.EmpresaId);
            
            string? afiliadoNome = null;
            if (assinatura.AfiliadoId.HasValue)
            {
                var afiliado = await _unitOfWork.Afiliados.GetByIdAsync(assinatura.AfiliadoId.Value);
                afiliadoNome = afiliado?.Nome;
            }
            
            // Re-fetch items if needed, or use what we passed? Service returns Assinatura, but usually doesn't reload items relation unless we ask or it's attached.
            // AssinaturaService.CriarAssinaturaAsync returns the created entity. 
            // The items are added to DB. The entity attached might have them in .Itens if EF tracked them on Add.
            // To be safe, let's fetch items using our new service method if they are missing.
            
            var items = assinatura.Itens?.ToList();
            if (items == null || !items.Any())
            {
                 items = (await _assinaturaService.ObterItensPorAssinaturaIdsAsync(new[] { assinatura.Id })).ToList();
            }

            var produtoIds = items.Select(i => i.ProdutoId).Distinct().ToList();
            var moduloIds = items.Where(i => i.ProdutoModuloId.HasValue).Select(i => i.ProdutoModuloId!.Value).Distinct().ToList();
            
            var produtos = await _produtoService.ObterProdutosPorIdsAsync(produtoIds);
            var modulos = await _produtoService.ObterModulosPorIdsAsync(moduloIds);

            var produtoLookup = produtos.ToDictionary(p => p.Id, p => p.Nome);
            var moduloLookup = modulos.ToDictionary(m => m.Id, m => m.Nome);

            // Group items
            var groupedItems = items.GroupBy(i => i.ProdutoId);
            var produtosDto = new List<AssinaturaProdutoDto>();

            foreach(var g in groupedItems)
            {
                var prodName = produtoLookup.ContainsKey(g.Key) ? produtoLookup[g.Key] : string.Empty;
                var modulosDto = new List<AssinaturaModuloDto>();
                foreach(var item in g)
                {
                    if(item.ProdutoModuloId.HasValue && moduloLookup.ContainsKey(item.ProdutoModuloId.Value))
                    {
                        modulosDto.Add(new AssinaturaModuloDto{ Id = item.ProdutoModuloId.Value, Nome = moduloLookup[item.ProdutoModuloId.Value] });
                    }
                }
                produtosDto.Add(new AssinaturaProdutoDto
                {
                    ProdutoId = g.Key,
                    ProdutoNome = prodName,
                    Modulos = modulosDto
                });
            }

            int? recorrenciaMeses = null;
            if(assinatura.IntervaloDias.HasValue)
            {
                 recorrenciaMeses = assinatura.IntervaloDias.Value / 30;
                 if(recorrenciaMeses == 0) recorrenciaMeses = 1;
            }

            return new AssinaturaDto
            {
                Id = assinatura.Id,
                ClienteId = assinatura.ClienteId,
                ClienteNome = cliente?.Nome ?? string.Empty,
                DataInicio = assinatura.DataInicio,
                DataFim = assinatura.DataFim,
                IsRecorrente = assinatura.IsRecorrente,
                IntervaloDias = assinatura.IntervaloDias,
                RecorrenciaMeses = recorrenciaMeses,
                Ativo = assinatura.Ativo,
                GerarFinanceiro = assinatura.GerarFinanceiro,
                GerarImplantacao = assinatura.GerarImplantacao,
                FormaPagamento = assinatura.FormaPagamento,
                FormaPagamentoNome = assinatura.FormaPagamento?.ToString(),
                AfiliadoId = assinatura.AfiliadoId,
                AfiliadoNome = afiliadoNome,
                Produtos = produtosDto
            };
        }
    }
}
