using Elo.Domain.Entities;
using Elo.Domain.Enums;
using BCrypt.Net;

namespace Elo.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Verificar se já existe usuário admin
        if (context.Users.Any())
        {
            return; // DB já foi inicializado
        }

        // Criar usuário administrador padrão
        var adminUser = new User
        {
            Nome = "Administrador",
            Email = "admin@elo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }
}
