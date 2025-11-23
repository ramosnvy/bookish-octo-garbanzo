namespace Elo.Domain.Exceptions;

public class FornecedorCategoriaNaoEncontradaException : DomainException
{
    public FornecedorCategoriaNaoEncontradaException(int id)
        : base($"Categoria de fornecedor com ID {id} não foi encontrada")
    {
    }
}
