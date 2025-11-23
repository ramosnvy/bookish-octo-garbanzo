using MediatR;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Users;

public static class ChangePassword
{
    public class Command : IRequest<bool>
    {
        public int Id { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
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
            return await _userService.AlterarSenhaAsync(
                request.Id,
                request.CurrentPassword,
                request.NewPassword
            );
        }
    }
}

