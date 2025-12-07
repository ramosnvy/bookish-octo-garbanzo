using Elo.Domain.Entities;
using Elo.Domain.Enums;
using BCrypt.Net;

namespace Elo.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        var hasChanges = false;

        if (!context.Users.Any())
        {
            var adminUser = new User
            {
                Nome = "Administrador",
                Email = "admin@elo.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            hasChanges = true;
        }

        if (!context.HistoriaStatuses.Any())
        {
            context.HistoriaStatuses.AddRange(new[]
            {
                new HistoriaStatus { Nome = "Pendente", Ordem = 1 },
                new HistoriaStatus { Nome = "Em andamento", Ordem = 2 },
                new HistoriaStatus { Nome = "Concluída", Ordem = 3, FechaHistoria = true },
                new HistoriaStatus { Nome = "Cancelada", Ordem = 4, FechaHistoria = true },
                new HistoriaStatus { Nome = "Pausada", Ordem = 5 }
            });
            hasChanges = true;
        }

        if (!context.HistoriaTipos.Any())
        {
            context.HistoriaTipos.AddRange(new[]
            {
                new HistoriaTipo { Nome = "Projeto", Ordem = 1 },
                new HistoriaTipo { Nome = "Entrega", Ordem = 2 },
                new HistoriaTipo { Nome = "Operação", Ordem = 3 },
                new HistoriaTipo { Nome = "Implementação", Ordem = 4 },
                new HistoriaTipo { Nome = "Ordem de Serviço", Ordem = 5 }
            });
            hasChanges = true;
        }

        if (!context.TicketTipos.Any())
        {
            context.TicketTipos.AddRange(new[]
            {
                new TicketTipo { Nome = "Suporte", Ordem = 1 },
                new TicketTipo { Nome = "Bug", Ordem = 2 },
                new TicketTipo { Nome = "Melhoria", Ordem = 3 },
                new TicketTipo { Nome = "Dúvida", Ordem = 4 },
                new TicketTipo { Nome = "Incidente", Ordem = 5 }
            });
            hasChanges = true;
        }

        if (hasChanges)
        {
            await context.SaveChangesAsync();
        }
    }
}
