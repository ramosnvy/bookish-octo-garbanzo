using Elo.Domain.Entities;

namespace Elo.Domain.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    int GetUserIdFromToken(string token);
}
