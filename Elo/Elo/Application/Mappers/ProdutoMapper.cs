using Elo.Domain.Entities;
using Elo.Application.DTOs.Produto;
using System.Linq;

namespace Elo.Application.Mappers;

public class ProdutoMapper : IProdutoMapper
{
    public ProdutoDto ToDto(Produto produto)
    {
        return new ProdutoDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Descricao = produto.Descricao,
            ValorCusto = produto.ValorCusto,
            ValorRevenda = produto.ValorRevenda,
            MargemLucro = produto.MargemLucro,
            Ativo = produto.Ativo,
            CreatedAt = produto.CreatedAt,
            UpdatedAt = produto.UpdatedAt,
            FornecedorId = produto.FornecedorId,
            FornecedorNome = produto.Fornecedor?.Nome,
            Modulos = produto.Modulos?.Select(ToModuloDto).ToList() ?? new List<ProdutoModuloDto>(),
            ValorTotalComAdicionais = produto.ValorRevenda + (produto.Modulos?.Where(m => m.Ativo).Sum(m => m.ValorAdicional) ?? 0)
        };
    }

    public IEnumerable<ProdutoDto> ToDtoList(IEnumerable<Produto> produtos)
    {
        return produtos.Select(ToDto);
    }

    public Produto ToEntity(CreateProdutoDto dto)
    {
        return new Produto
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            ValorCusto = dto.ValorCusto,
            ValorRevenda = dto.ValorRevenda,
            Ativo = dto.Ativo,
            CreatedAt = DateTime.UtcNow,
            FornecedorId = dto.FornecedorId
        };
    }

    public void UpdateEntity(Produto entity, UpdateProdutoDto dto)
    {
        entity.Nome = dto.Nome;
        entity.Descricao = dto.Descricao;
        entity.ValorCusto = dto.ValorCusto;
        entity.ValorRevenda = dto.ValorRevenda;
        entity.Ativo = dto.Ativo;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.FornecedorId = dto.FornecedorId;
    }

    private ProdutoModuloDto ToModuloDto(ProdutoModulo modulo)
    {
        return new ProdutoModuloDto
        {
            Id = modulo.Id,
            Nome = modulo.Nome,
            Descricao = modulo.Descricao,
            ValorAdicional = modulo.ValorAdicional,
            CustoAdicional = modulo.CustoAdicional,
            Ativo = modulo.Ativo
        };
    }
}
