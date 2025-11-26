using System.Collections.Generic;
using System.Linq;
using Elo.Domain.Enums;

namespace Elo.Application.DTOs.Historia;

public class HistoriaDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public HistoriaStatus Status { get; set; }
    public HistoriaTipo Tipo { get; set; }
    public int UsuarioResponsavelId { get; set; }
    public string UsuarioResponsavelNome { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<HistoriaMovimentacaoDto> Movimentacoes { get; set; } = Enumerable.Empty<HistoriaMovimentacaoDto>();
    public IEnumerable<HistoriaProdutoDto> Produtos { get; set; } = Enumerable.Empty<HistoriaProdutoDto>();
}

public class CreateHistoriaDto
{
    public int ClienteId { get; set; }
    public HistoriaStatus Status { get; set; } = HistoriaStatus.Pendente;
    public HistoriaTipo Tipo { get; set; } = HistoriaTipo.Projeto;
    public int UsuarioResponsavelId { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public string? Observacoes { get; set; }
    public IEnumerable<HistoriaProdutoInputDto> Produtos { get; set; } = Enumerable.Empty<HistoriaProdutoInputDto>();
}

public class UpdateHistoriaDto : CreateHistoriaDto
{
    public int Id { get; set; }
}

public class HistoriaProdutoDto
{
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public IEnumerable<int> ProdutoModuloIds { get; set; } = Enumerable.Empty<int>();
    public IEnumerable<string> ProdutoModuloNomes { get; set; } = Enumerable.Empty<string>();
}

public class HistoriaProdutoInputDto
{
    public int ProdutoId { get; set; }
    public IEnumerable<int> ProdutoModuloIds { get; set; } = Enumerable.Empty<int>();
}

public class HistoriaMovimentacaoDto
{
    public int Id { get; set; }
    public int HistoriaId { get; set; }
    public HistoriaStatus StatusAnterior { get; set; }
    public HistoriaStatus StatusNovo { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public DateTime DataMovimentacao { get; set; }
    public string? Observacoes { get; set; }
}

public class CreateHistoriaMovimentacaoDto
{
    public HistoriaStatus StatusNovo { get; set; }
    public string? Observacoes { get; set; }
}
