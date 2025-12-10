using Elo.Domain.Enums;

namespace Elo.Application.DTOs.EmpresaFormaPagamento;

public class EmpresaFormaPagamentoDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public string FormaPagamentoNome { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public bool AVista { get; set; }
    public bool Ativo { get; set; }
    public DateTime CreatedAt { get; set; }
}
