namespace Elo.Domain.Entities;

public class Empresa
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string EmailContato { get; set; } = string.Empty;
    public string TelefoneContato { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<User> Usuarios { get; set; } = new List<User>();
}
