using Elo.Domain.Enums;

namespace Elo.Application.DTOs.Fornecedor;

public class FornecedorDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public int? CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public Status Status { get; set; }
    public ServicoPagamentoTipo TipoPagamentoServico { get; set; }
    public int PrazoPagamentoDias { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<FornecedorEnderecoDto> Enderecos { get; set; } = Enumerable.Empty<FornecedorEnderecoDto>();
}

public class CreateFornecedorDto
{
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public Status Status { get; set; } = Status.Ativo;
    public string TipoPagamentoServico { get; set; } = ServicoPagamentoTipo.PrePago.ToString();
    public int PrazoPagamentoDias { get; set; }
    public IEnumerable<FornecedorEnderecoInputDto> Enderecos { get; set; } = Enumerable.Empty<FornecedorEnderecoInputDto>();
}

public class UpdateFornecedorDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public Status Status { get; set; }
    public string TipoPagamentoServico { get; set; } = ServicoPagamentoTipo.PrePago.ToString();
    public int PrazoPagamentoDias { get; set; }
    public IEnumerable<FornecedorEnderecoInputDto> Enderecos { get; set; } = Enumerable.Empty<FornecedorEnderecoInputDto>();
}

public class FornecedorEnderecoDto
{
    public int Id { get; set; }
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
}

public class FornecedorEnderecoInputDto
{
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
}
