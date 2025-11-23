namespace Elo.Application.DTOs.Produto;

public class ProdutoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorCusto { get; set; }
    public decimal ValorRevenda { get; set; }
    public decimal MargemLucro { get; set; }
    public bool Ativo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public decimal ValorTotalComAdicionais { get; set; }
    public IEnumerable<ProdutoModuloDto> Modulos { get; set; } = new List<ProdutoModuloDto>();
}

public class CreateProdutoDto
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorCusto { get; set; }
    public decimal ValorRevenda { get; set; }
    public bool Ativo { get; set; } = true;
    public int? FornecedorId { get; set; }
    public IEnumerable<ProdutoModuloInputDto> Modulos { get; set; } = new List<ProdutoModuloInputDto>();
}

public class UpdateProdutoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorCusto { get; set; }
    public decimal ValorRevenda { get; set; }
    public bool Ativo { get; set; }
    public int? FornecedorId { get; set; }
    public IEnumerable<ProdutoModuloInputDto> Modulos { get; set; } = new List<ProdutoModuloInputDto>();
}

public class CalcularMargemDto
{
    public decimal ValorCusto { get; set; }
    public decimal ValorRevenda { get; set; }
}

public class ProdutoModuloDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorAdicional { get; set; }
    public decimal CustoAdicional { get; set; }
    public bool Ativo { get; set; }
}

public class ProdutoModuloInputDto
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorAdicional { get; set; }
    public decimal CustoAdicional { get; set; }
    public bool Ativo { get; set; } = true;
}
