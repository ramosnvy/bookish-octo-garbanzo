namespace Elo.Application.DTOs.Fornecedor;

public class FornecedorCategoriaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}

public class CreateFornecedorCategoriaDto
{
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}

public class UpdateFornecedorCategoriaDto : CreateFornecedorCategoriaDto
{
    public int Id { get; set; }
}
