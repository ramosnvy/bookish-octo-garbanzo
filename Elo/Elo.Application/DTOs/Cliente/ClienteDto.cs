using Elo.Domain.Enums;

namespace Elo.Application.DTOs.Cliente;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CnpjCpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public Status Status { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<ClienteEnderecoDto> Enderecos { get; set; } = Enumerable.Empty<ClienteEnderecoDto>();
}

public class ClienteEnderecoDto
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

public class ClienteEnderecoInputDto
{
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
}

public class CreateClienteDto
{
    public string Nome { get; set; } = string.Empty;
    public string CnpjCpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public Status Status { get; set; } = Status.Ativo;
    public IEnumerable<ClienteEnderecoInputDto> Enderecos { get; set; } = Enumerable.Empty<ClienteEnderecoInputDto>();
}

public class UpdateClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CnpjCpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public Status Status { get; set; }
    public IEnumerable<ClienteEnderecoInputDto> Enderecos { get; set; } = Enumerable.Empty<ClienteEnderecoInputDto>();
}
