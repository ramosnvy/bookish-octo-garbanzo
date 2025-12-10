namespace Elo.Application.DTOs.Cliente;

/// <summary>
/// DTO simplificado para listagem de clientes (sem endereços completos)
/// </summary>
public class ClienteListDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CnpjCpf { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public Domain.Enums.Status Status { get; set; }
    public string StatusNome { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Informações resumidas
    public int QuantidadeEnderecos { get; set; }
    public string? CidadePrincipal { get; set; }
    public string? EstadoPrincipal { get; set; }
}
