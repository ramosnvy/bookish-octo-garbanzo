using Elo.Domain.Entities;
using Elo.Application.DTOs.Fornecedor;

namespace Elo.Application.Mappers;

public interface IFornecedorMapper
{
    FornecedorDto ToDto(Pessoa pessoa);
    IEnumerable<FornecedorDto> ToDtoList(IEnumerable<Pessoa> pessoas);
}
