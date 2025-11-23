using Elo.Domain.Entities;
using Elo.Application.DTOs.User;

namespace Elo.Application.Mappers;

public class UserMapper : IUserMapper
{
    public UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Nome = user.Nome,
            Email = user.Email,
            Role = user.Role,
            EmpresaId = user.EmpresaId,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public IEnumerable<UserDto> ToDtoList(IEnumerable<User> users)
    {
        return users.Select(ToDto);
    }

    public User ToEntity(CreateUserDto dto)
    {
        return new User
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Role = dto.Role,
            EmpresaId = dto.EmpresaId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateEntity(User entity, UpdateUserDto dto)
    {
        entity.Nome = dto.Nome;
        entity.Email = dto.Email;
        entity.Role = dto.Role;
        entity.EmpresaId = dto.EmpresaId;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
