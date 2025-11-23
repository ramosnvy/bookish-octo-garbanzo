namespace Elo.Domain.Entities;

public class PessoaEndereco
{
    public int Id { get; set; }
    public int PessoaId { get; set; }
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;

    public Pessoa? Pessoa { get; set; }
}
