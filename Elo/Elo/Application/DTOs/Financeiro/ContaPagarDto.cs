using Elo.Domain.Enums;

namespace Elo.Application.DTOs.Financeiro;

public class ContaPagarDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int FornecedorId { get; set; }
    public string FornecedorNome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public ContaStatus Status { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool IsRecorrente { get; set; }
    public int TotalParcelas { get; set; }
    public int IntervaloDias { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<ContaPagarItemDto> Itens { get; set; } = new List<ContaPagarItemDto>();
    public IEnumerable<ContaPagarParcelaDto> Parcelas { get; set; } = new List<ContaPagarParcelaDto>();
}

public class CreateContaPagarDto
{
    public int FornecedorId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public ContaStatus Status { get; set; } = ContaStatus.Pendente;
    public string Categoria { get; set; } = string.Empty;
    public bool IsRecorrente { get; set; }
    public int? NumeroParcelas { get; set; }
    public int? IntervaloDias { get; set; }
    public IEnumerable<ContaFinanceiraItemInputDto> Itens { get; set; } = new List<ContaFinanceiraItemInputDto>();
}

public class UpdateContaPagarDto : CreateContaPagarDto
{
    public int Id { get; set; }
}

public class ContaPagarItemDto
{
    public int Id { get; set; }
    public int ContaPagarId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int? ProdutoId { get; set; }
    public int? ProdutoModuloId { get; set; }
    public IEnumerable<int> ProdutoModuloIds { get; set; } = new List<int>();
}

public class ContaPagarParcelaDto
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public ContaStatus Status { get; set; }
}

public class ContaFinanceiraItemInputDto
{
    public int? ProdutoId { get; set; }
    public int? ProdutoModuloId { get; set; }
    public IEnumerable<int>? ProdutoModuloIds { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
