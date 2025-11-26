using System.Collections.Generic;
using System.Linq;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Enums;

namespace Elo.Application.UseCases.Historias;

internal static class HistoriaMapper
{
    public static HistoriaDto ToDto(
        Historia historia,
        IReadOnlyDictionary<int, Pessoa> clientes,
        IReadOnlyDictionary<int, Produto> produtos,
        IReadOnlyDictionary<int, ProdutoModulo> modulos,
        IReadOnlyDictionary<int, User> usuarios,
        IReadOnlyDictionary<int, List<HistoriaProduto>> produtosLookup,
        IReadOnlyDictionary<int, List<HistoriaMovimentacao>> movimentosLookup)
    {
        clientes.TryGetValue(historia.ClienteId, out var cliente);
        produtos.TryGetValue(historia.ProdutoId, out var produtoPrincipal);
        usuarios.TryGetValue(historia.UsuarioResponsavelId, out var responsavel);

        var historicoProdutos = produtosLookup.TryGetValue(historia.Id, out var associados)
            ? associados
            : new List<HistoriaProduto>();

        var produtoDtos = historicoProdutos.Select(hp =>
        {
            produtos.TryGetValue(hp.ProdutoId, out var produto);
            var moduloNomes = hp.ProdutoModuloIds
                .Where(id => modulos.ContainsKey(id))
                .Select(id => modulos[id].Nome)
                .ToList();

            return new HistoriaProdutoDto
            {
                ProdutoId = hp.ProdutoId,
                ProdutoNome = produto?.Nome ?? string.Empty,
                ProdutoModuloIds = hp.ProdutoModuloIds.ToList(),
                ProdutoModuloNomes = moduloNomes
            };
        }).ToList();

        var primaryProduto = produtoDtos.FirstOrDefault();
        var primaryProdutoId = primaryProduto?.ProdutoId ?? historia.ProdutoId;
        var primaryProdutoNome = primaryProduto?.ProdutoNome ?? produtoPrincipal?.Nome ?? string.Empty;

        var movimentacoes = movimentosLookup.TryGetValue(historia.Id, out var movs)
            ? movs
            : Enumerable.Empty<HistoriaMovimentacao>();
        var movimentacoesDto = movimentacoes.Select(m =>
        {
            usuarios.TryGetValue(m.UsuarioId, out var usuarioMovimentacao);
            return new HistoriaMovimentacaoDto
            {
                Id = m.Id,
                HistoriaId = m.HistoriaId,
                StatusAnterior = m.StatusAnterior,
                StatusNovo = m.StatusNovo,
                UsuarioId = m.UsuarioId,
                UsuarioNome = usuarioMovimentacao?.Nome ?? string.Empty,
                DataMovimentacao = m.DataMovimentacao,
                Observacoes = m.Observacoes
            };
        }).ToList();

        return new HistoriaDto
        {
            Id = historia.Id,
            ClienteId = historia.ClienteId,
            ClienteNome = cliente?.Nome ?? string.Empty,
            ProdutoId = primaryProdutoId,
            ProdutoNome = primaryProdutoNome,
            Status = historia.Status,
            Tipo = historia.Tipo,
            UsuarioResponsavelId = historia.UsuarioResponsavelId,
            UsuarioResponsavelNome = responsavel?.Nome ?? string.Empty,
            DataInicio = historia.DataInicio,
            DataFinalizacao = historia.DataFinalizacao,
            Observacoes = historia.Observacoes,
            CreatedAt = historia.CreatedAt,
            UpdatedAt = historia.UpdatedAt,
            Movimentacoes = movimentacoesDto,
            Produtos = produtoDtos
        };
    }
}
