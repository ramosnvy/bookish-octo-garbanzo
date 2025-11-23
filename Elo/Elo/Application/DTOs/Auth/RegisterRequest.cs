using Elo.Domain.Enums;

namespace Elo.Application.DTOs.Auth;

public class RegisterRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
}
