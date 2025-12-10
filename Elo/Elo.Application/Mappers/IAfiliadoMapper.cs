using Elo.Domain.Entities;

namespace Elo.Application.Mappers;

public interface IAfiliadoMapper
{
    Application.DTOs.Afiliado.AfiliadoDto ToDto(Afiliado afiliado);
    IEnumerable<Application.DTOs.Afiliado.AfiliadoDto> ToDtoList(IEnumerable<Afiliado> afiliados);
}
