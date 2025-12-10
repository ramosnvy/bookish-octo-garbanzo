using Elo.Domain.Entities;
using Elo.Application.DTOs.Afiliado;

namespace Elo.Application.Mappers;

public class AfiliadoMapper : IAfiliadoMapper
{
    public AfiliadoDto ToDto(Afiliado afiliado)
    {
        return new AfiliadoDto
        {
            Id = afiliado.Id,
            Nome = afiliado.Nome,
            Email = afiliado.Email,
            Documento = afiliado.Documento,
            Telefone = afiliado.Telefone,
            Porcentagem = afiliado.Porcentagem,
            Status = afiliado.Status,
            CreatedAt = afiliado.CreatedAt,
            UpdatedAt = afiliado.UpdatedAt
        };
    }

    public IEnumerable<AfiliadoDto> ToDtoList(IEnumerable<Afiliado> afiliados)
    {
        return afiliados.Select(ToDto);
    }
}
