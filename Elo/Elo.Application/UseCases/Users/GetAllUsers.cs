using MediatR;
using Elo.Application.DTOs.User;
using Elo.Application.Mappers;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Users;

public static class GetAllUsers
{
    public class Query : IRequest<IEnumerable<UserDto>>
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? Search { get; set; }
        public string? Role { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<UserDto>>
    {
        private readonly IUserService _userService;
        private readonly IUserMapper _userMapper;

        public Handler(IUserService userService, IUserMapper userMapper)
        {
            _userService = userService;
            _userMapper = userMapper;
        }

        public async Task<IEnumerable<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var users = await _userService.ObterTodosUsuariosAsync(request.EmpresaId);
            return _userMapper.ToDtoList(users);
        }
    }
}

