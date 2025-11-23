using Elo.Domain.Entities;
using Elo.Application.DTOs.Cliente;

namespace Elo.Application.Mappers;

public class ClienteMapper : IClienteMapper
{
    public ClienteDto ToDto(Pessoa pessoa)
    {
        return new ClienteDto
        {
            Id = pessoa.Id,
            Nome = pessoa.Nome,
            CnpjCpf = pessoa.Documento,
            Email = pessoa.Email,
            Telefone = pessoa.Telefone,
            Status = pessoa.Status,
            DataCadastro = pessoa.DataCadastro,
            UpdatedAt = pessoa.UpdatedAt,
            Enderecos = pessoa.Enderecos?.Select(e => new ClienteEnderecoDto
            {
                Id = e.Id,
                Logradouro = e.Logradouro,
                Numero = e.Numero,
                Bairro = e.Bairro,
                Cidade = e.Cidade,
                Estado = e.Estado,
                Cep = e.Cep,
                Complemento = e.Complemento
            }) ?? Enumerable.Empty<ClienteEnderecoDto>()
        };
    }

    public IEnumerable<ClienteDto> ToDtoList(IEnumerable<Pessoa> pessoas)
    {
        return pessoas.Select(ToDto);
    }
}
