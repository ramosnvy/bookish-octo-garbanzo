using Elo.Application.DTOs.Assinatura;
using Elo.Domain.Interfaces;
using MediatR;
using System.Text.Json.Serialization;

namespace Elo.Application.UseCases.Assinaturas;

public static class GetAllAssinaturas
{
    public record Query : IRequest<IEnumerable<AssinaturaDto>>
    {
        [JsonIgnore]
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<AssinaturaDto>>
    {
        private readonly IAssinaturaService _assinaturaService;
        private readonly IPessoaService _pessoaService;
        private readonly IProdutoService _produtoService;

        public Handler(
            IAssinaturaService assinaturaService,
            IPessoaService pessoaService,
            IProdutoService produtoService)
        {
            _assinaturaService = assinaturaService;
            _pessoaService = pessoaService;
            _produtoService = produtoService;
        }

        public async Task<IEnumerable<AssinaturaDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var assinaturas = (await _assinaturaService.ObterAssinaturasAsync(request.EmpresaId)).ToList();

            if (!assinaturas.Any())
                return Enumerable.Empty<AssinaturaDto>();

            var assinaturaIds = assinaturas.Select(a => a.Id).ToList();

            // Fetch Items Async
            // Fetch Items Async (Wait immediately)
            var items = (await _assinaturaService.ObterItensPorAssinaturaIdsAsync(assinaturaIds)).ToList();
            
            // Fetch Client Names
            var clienteIds = assinaturas.Select(a => a.ClienteId).Distinct().ToList();
            var clientes = await _pessoaService.ObterPessoasPorIdsAsync(clienteIds, request.EmpresaId);

            var itemLookup = items.GroupBy(i => i.AssinaturaId).ToDictionary(g => g.Key, g => g.ToList());

            // Check what products/modules we need
            var produtoIds = items.Select(i => i.ProdutoId).Distinct().ToList();
            var moduloIds = items.Where(i => i.ProdutoModuloId.HasValue).Select(i => i.ProdutoModuloId!.Value).Distinct().ToList();

            var produtos = await _produtoService.ObterProdutosPorIdsAsync(produtoIds);
            var modulos = await _produtoService.ObterModulosPorIdsAsync(moduloIds);

            // Create Lookups
            var clientLookup = clientes.ToDictionary(c => c.Id, c => c.Nome);
            var produtoLookup = produtos.ToDictionary(p => p.Id, p => p.Nome);
            var moduloLookup = modulos.ToDictionary(m => m.Id, m => m.Nome);

            var result = new List<AssinaturaDto>();

            foreach(var a in assinaturas)
            {
                 var listaItens = itemLookup.ContainsKey(a.Id) ? itemLookup[a.Id] : new List<Domain.Entities.AssinaturaItem>();
                 
                 // Group items by Product
                 var groupedItems = listaItens.GroupBy(i => i.ProdutoId);
                 var produtosDto = new List<AssinaturaProdutoDto>();

                 foreach(var g in groupedItems)
                 {
                     var prodName = produtoLookup.ContainsKey(g.Key) ? produtoLookup[g.Key] : "Produto Desconhecido";
                     
                     var modulosDto = new List<AssinaturaModuloDto>();

                     foreach(var item in g)
                     {
                         if(item.ProdutoModuloId.HasValue && moduloLookup.ContainsKey(item.ProdutoModuloId.Value))
                         {
                             modulosDto.Add(new AssinaturaModuloDto 
                             { 
                                 Id = item.ProdutoModuloId.Value, 
                                 Nome = moduloLookup[item.ProdutoModuloId.Value] 
                             });
                         }
                     }
                     
                     produtosDto.Add(new AssinaturaProdutoDto
                     {
                         ProdutoId = g.Key,
                         ProdutoNome = prodName,
                         Modulos = modulosDto
                     });
                 }

                 // RecorrenciaMeses logic:
                 int? recorrenciaMeses = null;
                 if(a.IntervaloDias.HasValue)
                 {
                     recorrenciaMeses = a.IntervaloDias.Value / 30;
                     if(recorrenciaMeses == 0) recorrenciaMeses = 1; 
                 }

                 result.Add(new AssinaturaDto
                 {
                     Id = a.Id,
                     ClienteId = a.ClienteId,
                     ClienteNome = clientLookup.ContainsKey(a.ClienteId) ? clientLookup[a.ClienteId] : string.Empty,
                     DataInicio = a.DataInicio,
                     DataFim = a.DataFim,
                     IsRecorrente = a.IsRecorrente,
                     IntervaloDias = a.IntervaloDias,
                     RecorrenciaMeses = recorrenciaMeses,
                     Ativo = a.Ativo,
                     GerarFinanceiro = a.GerarFinanceiro,
                     GerarImplantacao = a.GerarImplantacao,
                     Produtos = produtosDto
                 });
            }

            return result;
        }
    }
}
