namespace Elo.Application.Common;

/// <summary>
/// Representa um resultado paginado genérico
/// </summary>
/// <typeparam name="T">Tipo dos itens da página</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Lista de itens da página atual
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Número total de itens (em todas as páginas)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Número da página atual (1-indexed)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Tamanho da página (quantidade de itens por página)
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Número total de páginas
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indica se existe página anterior
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Indica se existe próxima página
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;

    /// <summary>
    /// Construtor padrão
    /// </summary>
    public PagedResult()
    {
    }

    /// <summary>
    /// Construtor com parâmetros
    /// </summary>
    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Cria um resultado paginado vazio
    /// </summary>
    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PagedResult<T>(Enumerable.Empty<T>(), 0, pageNumber, pageSize);
    }
}
