using MediatR;
using Elo.Application.DTOs.User;
using Elo.Application.Mappers;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Users;

public static class GetUserById
{
    public class Query : IRequest<UserDto?>
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
        public bool IsGlobalAdmin { get; set; }
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
            var user = await _userService.ObterUsuarioPorIdAsync(request.Id);
            
            if (user == null)
            {
                return null;
            }

            if (!request.IsGlobalAdmin && request.EmpresaId.HasValue && user.EmpresaId != request.EmpresaId)
            {
                return null;
            }

            return _userMapper.ToDto(user);
        }
    }
}

