using MediatR;
using Elo.Application.DTOs.User;
using Elo.Application.Mappers;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Auth;

public static class GetMe
{
    public class Query : IRequest<UserDto?>
    {
        public int UserId { get; set; }
    }

    public class Handler : IRequestHandler<Query, UserDto?>
    {
        private readonly IUserService _userService;
        private readonly IUserMapper _userMapper;

        public Handler(IUserService userService, IUserMapper userMapper)
        {
            _userService = userService;
            _userMapper = userMapper;
        }

        public async Task<UserDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _userService.ObterUsuarioPorIdAsync(request.UserId);

            return user == null ? null : _userMapper.ToDto(user);
        }
    }
}
