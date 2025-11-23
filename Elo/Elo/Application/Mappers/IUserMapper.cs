using Elo.Domain.Entities;
using Elo.Application.DTOs.User;

namespace Elo.Application.Mappers;

public interface IUserMapper
{
    UserDto ToDto(User user);
    IEnumerable<UserDto> ToDtoList(IEnumerable<User> users);
    User ToEntity(CreateUserDto dto);
    void UpdateEntity(User entity, UpdateUserDto dto);
}
