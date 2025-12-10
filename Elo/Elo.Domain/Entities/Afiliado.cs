using Elo.Domain.Enums;

namespace Elo.Domain.Entities;

public class Afiliado
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public decimal Porcentagem { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
