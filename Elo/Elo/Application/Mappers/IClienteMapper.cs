using Elo.Domain.Entities;
using Elo.Application.DTOs.Cliente;

namespace Elo.Application.Mappers;

public interface IClienteMapper
{
    ClienteDto ToDto(Pessoa pessoa);
    IEnumerable<ClienteDto> ToDtoList(IEnumerable<Pessoa> pessoas);
}
