using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Users;

public static class DeleteUser
{
    public class Command : IRequest<bool>
    {
        public int Id { get; set; }
        public bool IsGlobalAdmin { get; set; }
        public int? RequesterEmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IUserService _userService;

        public Handler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _userService.ObterUsuarioPorIdAsync(request.Id);
            if (user == null)
            {
                return false;
            }

            if (!request.IsGlobalAdmin)
            {
                if (!request.RequesterEmpresaId.HasValue || user.EmpresaId != request.RequesterEmpresaId)
                {
                    throw new UnauthorizedAccessException("Usuário não pertence à mesma empresa.");
                }
            }

            return await _userService.DeletarUsuarioAsync(request.Id);
        }
    }
}

