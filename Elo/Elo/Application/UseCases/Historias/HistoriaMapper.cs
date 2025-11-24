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
        IReadOnlyDictionary<int, User> usuarios,
        IReadOnlyDictionary<int, List<HistoriaMovimentacao>> movimentosLookup)
    {
        clientes.TryGetValue(historia.ClienteId, out var cliente);
        produtos.TryGetValue(historia.ProdutoId, out var produto);
        usuarios.TryGetValue(historia.UsuarioResponsavelId, out var responsavel);

        var movimentacoes = movimentosLookup.TryGetValue(historia.Id, out var movimentos)
            ? movimentos
                .OrderByDescending(m => m.DataMovimentacao)
                .Select(m =>
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
                })
            : Enumerable.Empty<HistoriaMovimentacaoDto>();

        return new HistoriaDto
        {
            Id = historia.Id,
            ClienteId = historia.ClienteId,
            ClienteNome = cliente?.Nome ?? string.Empty,
            ProdutoId = historia.ProdutoId,
            ProdutoNome = produto?.Nome ?? string.Empty,
            Status = historia.Status,
            Tipo = historia.Tipo,
            UsuarioResponsavelId = historia.UsuarioResponsavelId,
            UsuarioResponsavelNome = responsavel?.Nome ?? string.Empty,
            DataInicio = historia.DataInicio,
            DataFinalizacao = historia.DataFinalizacao,
            Observacoes = historia.Observacoes,
            CreatedAt = historia.CreatedAt,
            UpdatedAt = historia.UpdatedAt,
            Movimentacoes = movimentacoes
        };
    }
}
