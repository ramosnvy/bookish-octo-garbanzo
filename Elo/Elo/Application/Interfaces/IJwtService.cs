using Elo.Domain.Entities;

namespace Elo.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    int GetUserIdFromToken(string token);
}
