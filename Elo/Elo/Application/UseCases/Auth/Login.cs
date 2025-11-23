using MediatR;
using Elo.Application.DTOs.Auth;
using Elo.Application.Interfaces;
using Elo.Domain.Interfaces.Repositories;

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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        public Handler(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
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
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            };
        }
    }
}
