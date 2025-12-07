using System.Collections.Generic;
using System.Linq;

namespace Elo.Application.DTOs.Historia;

public class HistoriaDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string StatusNome { get; set; } = string.Empty;
    public string? StatusCor { get; set; }
    public bool StatusFechaHistoria { get; set; }
    public int TipoId { get; set; }
    public string TipoNome { get; set; } = string.Empty;
    public string? TipoDescricao { get; set; }
    public int? UsuarioResponsavelId { get; set; }
    public string UsuarioResponsavelNome { get; set; } = string.Empty;
    public int? PrevisaoDias { get; set; }
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
    public int StatusId { get; set; }
    public int TipoId { get; set; }
    public int? UsuarioResponsavelId { get; set; }
    public int? PrevisaoDias { get; set; }
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
    public int StatusAnteriorId { get; set; }
    public string StatusAnteriorNome { get; set; } = string.Empty;
    public int StatusNovoId { get; set; }
    public string StatusNovoNome { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public DateTime DataMovimentacao { get; set; }
    public string? Observacoes { get; set; }
}

public class CreateHistoriaMovimentacaoDto
{
    public int StatusNovoId { get; set; }
    public string? Observacoes { get; set; }
}

public class HistoriaStatusDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Cor { get; set; }
    public bool FechaHistoria { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateHistoriaStatusDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Cor { get; set; }
    public bool FechaHistoria { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}

public class UpdateHistoriaStatusDto : CreateHistoriaStatusDto
{
    public int Id { get; set; }
}

public class HistoriaTipoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateHistoriaTipoDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}

public class UpdateHistoriaTipoDto : CreateHistoriaTipoDto
{
    public int Id { get; set; }
}

public class HistoriaKanbanDto
{
    public IEnumerable<HistoriaDto> Historias { get; set; } = Enumerable.Empty<HistoriaDto>();
    public IEnumerable<HistoriaStatusDto> Statuses { get; set; } = Enumerable.Empty<HistoriaStatusDto>();
    public IEnumerable<HistoriaTipoDto> Tipos { get; set; } = Enumerable.Empty<HistoriaTipoDto>();
}
