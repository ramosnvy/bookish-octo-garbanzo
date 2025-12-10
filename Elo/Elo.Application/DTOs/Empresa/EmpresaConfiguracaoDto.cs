using Elo.Domain.Enums;

namespace Elo.Application.DTOs.Empresa;

public class EmpresaConfiguracaoDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public decimal JurosValor { get; set; }
    public TipoValor JurosTipo { get; set; }
    public decimal MoraValor { get; set; }
    public TipoValor MoraTipo { get; set; }
    public int DiaPagamentoAfiliado { get; set; }
}

public class UpdateEmpresaConfiguracaoDto
{
    public decimal JurosValor { get; set; }
    public TipoValor JurosTipo { get; set; }
    public decimal MoraValor { get; set; }
    public TipoValor MoraTipo { get; set; }
    public int DiaPagamentoAfiliado { get; set; }
}
