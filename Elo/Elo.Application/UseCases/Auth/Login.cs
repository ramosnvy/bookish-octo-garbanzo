using MediatR;
using Elo.Application.DTOs.Auth;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Auth;

public static class Login
{
    public class Command : IRequest<LoginResponse>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, LoginResponse>
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public Handler(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _userService.ValidarCredenciaisAsync(request.Email, request.Password);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Email ou senha invalidos");
            }

            var token = _jwtService.GenerateToken(user);

            return new LoginResponse
            {
                Token = token,
                Nome = user.Nome,
                Email = user.Email,
                Role = user.Role.ToString(),
                EmpresaId = user.EmpresaId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Should ideally come from JwtService or config, but hardcoded in original service anyway.
            };
        }
    }
}
