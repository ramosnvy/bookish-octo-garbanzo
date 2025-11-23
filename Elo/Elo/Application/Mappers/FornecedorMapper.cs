using Elo.Domain.Entities;
using Elo.Application.DTOs.Fornecedor;

namespace Elo.Application.Mappers;

public class FornecedorMapper : IFornecedorMapper
{
    public FornecedorDto ToDto(Pessoa pessoa)
    {
        return new FornecedorDto
        {
            Id = pessoa.Id,
            Nome = pessoa.Nome,
            Cnpj = pessoa.Documento,
            Email = pessoa.Email,
            Telefone = pessoa.Telefone,
            CategoriaId = pessoa.FornecedorCategoriaId,
            CategoriaNome = pessoa.FornecedorCategoria?.Nome ?? pessoa.Categoria,
            Status = pessoa.Status,
            DataCadastro = pessoa.DataCadastro,
            UpdatedAt = pessoa.UpdatedAt,
            Enderecos = pessoa.Enderecos?.Select(e => new FornecedorEnderecoDto
            {
                Id = e.Id,
                Logradouro = e.Logradouro,
                Numero = e.Numero,
                Bairro = e.Bairro,
                Cidade = e.Cidade,
                Estado = e.Estado,
                Cep = e.Cep,
                Complemento = e.Complemento
            }) ?? Enumerable.Empty<FornecedorEnderecoDto>()
        };
    }

    public IEnumerable<FornecedorDto> ToDtoList(IEnumerable<Pessoa> pessoas)
    {
        return pessoas.Select(ToDto);
    }
}
