using Elo.Domain.Entities;
using Elo.Application.DTOs.Produto;

namespace Elo.Application.Mappers;

public interface IProdutoMapper
{
    ProdutoDto ToDto(Produto produto);
    IEnumerable<ProdutoDto> ToDtoList(IEnumerable<Produto> produtos);
    Produto ToEntity(CreateProdutoDto dto);
    void UpdateEntity(Produto entity, UpdateProdutoDto dto);
}
