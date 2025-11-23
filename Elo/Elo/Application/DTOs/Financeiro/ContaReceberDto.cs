using Elo.Domain.Enums;

namespace Elo.Application.DTOs.Financeiro;

public class ContaReceberDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataRecebimento { get; set; }
    public ContaStatus Status { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsRecorrente { get; set; }
    public int TotalParcelas { get; set; }
    public int IntervaloDias { get; set; }
    public IEnumerable<ContaReceberItemDto> Itens { get; set; } = new List<ContaReceberItemDto>();
    public IEnumerable<ContaReceberParcelaDto> Parcelas { get; set; } = new List<ContaReceberParcelaDto>();
}

public class CreateContaReceberDto
{
    public int ClienteId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataRecebimento { get; set; }
    public ContaStatus Status { get; set; } = ContaStatus.Pendente;
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.PIX;
    public bool IsRecorrente { get; set; }
    public int? NumeroParcelas { get; set; }
    public int? IntervaloDias { get; set; }
    public IEnumerable<ContaFinanceiraItemInputDto> Itens { get; set; } = new List<ContaFinanceiraItemInputDto>();
}

public class UpdateContaReceberDto : CreateContaReceberDto
{
    public int Id { get; set; }
}

public class ContaReceberItemDto
{
    public int Id { get; set; }
    public int ContaReceberId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int? ProdutoId { get; set; }
    public int? ProdutoModuloId { get; set; }
    public IEnumerable<int> ProdutoModuloIds { get; set; } = new List<int>();
}

public class ContaReceberParcelaDto
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataRecebimento { get; set; }
    public ContaStatus Status { get; set; }
}
