namespace Elo.Application.DTOs.Empresa;

public class EmpresaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string EmailContato { get; set; } = string.Empty;
    public string TelefoneContato { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateEmpresaDto
{
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string EmailContato { get; set; } = string.Empty;
    public string TelefoneContato { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public InitialUserDto UsuarioInicial { get; set; } = new();
}

public class UpdateEmpresaDto
{
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string EmailContato { get; set; } = string.Empty;
    public string TelefoneContato { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}

public class InitialUserDto
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
