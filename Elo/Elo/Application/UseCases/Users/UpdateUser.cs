using MediatR;
using Elo.Application.DTOs.User;
using Elo.Application.Mappers;
using Elo.Domain.Interfaces;
using Elo.Domain.Exceptions;

namespace Elo.Application.UseCases.Users;

public static class UpdateUser
{
    public class Command : IRequest<UserDto>
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? EmpresaId { get; set; }
        public bool IsGlobalAdmin { get; set; }
        public int? RequesterEmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, UserDto>
    {
        private readonly IUserService _userService;
        private readonly IUserMapper _userMapper;

        public Handler(IUserService userService, IUserMapper userMapper)
        {
            _userService = userService;
            _userMapper = userMapper;
        }

        public async Task<UserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var existing = await _userService.ObterUsuarioPorIdAsync(request.Id);
            if (existing == null)
            {
                throw new ClienteNaoEncontradoException(request.Id);
            }

            var empresaId = request.EmpresaId;
            if (!request.IsGlobalAdmin)
            {
                if (!request.RequesterEmpresaId.HasValue)
                {
                    throw new UnauthorizedAccessException("Usuário não vinculado a uma empresa.");
                }

                if (existing.EmpresaId != request.RequesterEmpresaId)
                {
                    throw new UnauthorizedAccessException("Usuário não pertence à mesma empresa.");
                }

                empresaId = request.RequesterEmpresaId;
            }

            var user = await _userService.AtualizarUsuarioAsync(
                request.Id,
                request.Nome,
                request.Email,
                request.Role,
                empresaId
            );

            return _userMapper.ToDto(user);
        }
    }
}

